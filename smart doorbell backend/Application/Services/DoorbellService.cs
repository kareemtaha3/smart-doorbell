using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using smart_doorbell.Application.DTOs;
using smart_doorbell.Application.Interfaces;
using smart_doorbell.Application.Interfaces.Repositories;
using smart_doorbell.Application.Interfaces.Services;
using smart_doorbell.Domain.Entities;
using smart_doorbell.Domain.Enums;

namespace smart_doorbell.Application.Services;

public class DoorbellService : IDoorbellService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRedisSessionCache _redisCache;
    private readonly IMqttClientService _mqttService;
    private readonly IMinioStorageService _minioService;
    private readonly INotificationDispatcher _dispatcher;
    private readonly INotificationService _notificationService;
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DoorbellService> _logger;

    public DoorbellService(
        IUnitOfWork unitOfWork,
        IRedisSessionCache redisCache,
        IMqttClientService mqttService,
        IMinioStorageService minioService,
        INotificationDispatcher dispatcher,
        INotificationService notificationService,
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory,
        ILogger<DoorbellService> logger)
    {
        _unitOfWork = unitOfWork;
        _redisCache = redisCache;
        _mqttService = mqttService;
        _minioService = minioService;
        _dispatcher = dispatcher;
        _notificationService = notificationService;
        _configuration = configuration;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    private int SilenceTimeoutSeconds => int.TryParse(_configuration["DoorbellOptions:SilenceTimeoutSeconds"], out var s) ? s : 15;
    private int EscalationTimeoutSeconds => int.TryParse(_configuration["DoorbellOptions:EscalationTimeoutSeconds"], out var e) ? e : 30;

    public async Task<RingSessionDetailDto> ProcessRingEventAsync(Guid buttonDeviceId, string deviceEventId, CancellationToken cancellationToken = default)
    {
        var buttonDevice = await _unitOfWork.Devices.GetByIdAsync(buttonDeviceId, cancellationToken);
        if (buttonDevice == null || buttonDevice.Type != DeviceType.Button)
        {
            throw new KeyNotFoundException($"Button device with ID '{buttonDeviceId}' not found.");
        }

        var homeId = buttonDevice.HomeId;
        var existingSessionId = await _redisCache.GetActiveSessionIdAsync(buttonDeviceId, cancellationToken);

        if (existingSessionId.HasValue)
        {
            _logger.LogInformation("Existing session {SessionId} found in Redis for device {DeviceId}. Processing press count increment.", existingSessionId.Value, buttonDeviceId);

            var session = await _unitOfWork.RingSessions.GetByIdAsync(existingSessionId.Value, cancellationToken);
            if (session != null && session.ClosedAt == null)
            {
                session.PressCount += 1;
                session.LastPressAt = DateTime.UtcNow;

                var ringEvent = new RingEvent
                {
                    Id = Guid.NewGuid(),
                    SessionId = session.Id,
                    DeviceId = buttonDeviceId,
                    DeviceEventId = deviceEventId,
                    ReceivedAt = DateTime.UtcNow
                };
                await _unitOfWork.RingEvents.AddAsync(ringEvent, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);


                await _redisCache.RefreshSilenceTimerAsync(buttonDeviceId, TimeSpan.FromSeconds(SilenceTimeoutSeconds), cancellationToken);

                await TriggerChimeCommandAsync(homeId, ringEvent.Id, cancellationToken);

 
                var existingImageUrl = session.Notification?.ImageUrl;
                await _dispatcher.DispatchRingNotificationAsync(session.Id, homeId, session.PressCount, existingImageUrl);

                var dto = await _notificationService.GetSessionDetailsAsync(session.Id, cancellationToken);
                return dto!;
            }
        }

        _logger.LogInformation("No active session found in Redis for device {DeviceId}. Creating new RingSession.", buttonDeviceId);

        var newSession = new RingSession
        {
            Id = Guid.NewGuid(),
            ButtonDeviceId = buttonDeviceId,
            HomeId = homeId,
            CreatedAt = DateTime.UtcNow,
            LastPressAt = DateTime.UtcNow,
            PressCount = 1
        };

        await _unitOfWork.RingSessions.AddAsync(newSession, cancellationToken);

        var newRingEvent = new RingEvent
        {
            Id = Guid.NewGuid(),
            SessionId = newSession.Id,
            DeviceId = buttonDeviceId,
            DeviceEventId = deviceEventId,
            ReceivedAt = DateTime.UtcNow
        };
        await _unitOfWork.RingEvents.AddAsync(newRingEvent, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);


        await _redisCache.SetActiveSessionAsync(buttonDeviceId, newSession.Id, TimeSpan.FromSeconds(SilenceTimeoutSeconds), cancellationToken);

        await TriggerChimeCommandAsync(homeId, newRingEvent.Id, cancellationToken);
        await TriggerCameraSnapshotCommandAsync(homeId, newSession.Id, cancellationToken);


        await _dispatcher.DispatchRingNotificationAsync(newSession.Id, homeId, newSession.PressCount, null);


        _ = StartSessionTimersAsync(buttonDeviceId, newSession.Id);

        var sessionDetail = await _notificationService.GetSessionDetailsAsync(newSession.Id, cancellationToken);
        return sessionDetail!;
    }

    public async Task CloseSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _unitOfWork.RingSessions.GetByIdAsync(sessionId, cancellationToken);
        if (session != null && session.ClosedAt == null)
        {
            session.ClosedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _redisCache.RemoveSessionAsync(session.ButtonDeviceId, cancellationToken);
            _logger.LogInformation("Session {SessionId} closed in DB and purged from Redis.", sessionId);
        }
    }

    public async Task ProcessMinioUploadWebhookAsync(string objectKey, string bucket, CancellationToken cancellationToken = default)
    {
        var decodedKey = System.Net.WebUtility.UrlDecode(objectKey);
        _logger.LogInformation("Processing MinIO upload webhook for Bucket: '{Bucket}', ObjectKey: '{ObjectKey}' (Decoded: '{DecodedKey}')", bucket, objectKey, decodedKey);

        var snapshot = await _unitOfWork.CameraSnapshots.GetByObjectKeyAsync(decodedKey, cancellationToken)
            ?? await _unitOfWork.CameraSnapshots.GetByObjectKeyAsync(objectKey, cancellationToken);

       
        if (snapshot == null && decodedKey.StartsWith($"{bucket}/", StringComparison.OrdinalIgnoreCase))
        {
            var strippedKey = decodedKey.Substring(bucket.Length + 1);
            snapshot = await _unitOfWork.CameraSnapshots.GetByObjectKeyAsync(strippedKey, cancellationToken);
        }


        if (snapshot == null)
        {
            var filename = Path.GetFileNameWithoutExtension(decodedKey);
            if (Guid.TryParse(filename, out var sessionId))
            {
                snapshot = await _unitOfWork.CameraSnapshots.GetBySessionIdAsync(sessionId, cancellationToken);
            }
        }

        if (snapshot == null)
        {
            _logger.LogWarning("CameraSnapshot with ObjectKey '{ObjectKey}' or decoded '{DecodedKey}' not found in DB.", objectKey, decodedKey);
            return;
        }

        var mediaUrl = await _minioService.GetPresignedObjectUrlAsync(snapshot.ObjectKey, snapshot.Bucket, cancellationToken);
        snapshot.AckStatus = AckStatus.Acked;
        snapshot.ReadyAt = DateTime.UtcNow;
        snapshot.MediaUrl = mediaUrl;

        var notification = await _unitOfWork.Notifications.GetBySessionIdAsync(snapshot.SessionId, cancellationToken);
        if (notification != null)
        {
            notification.ImageUrl = mediaUrl;
            notification.UpdatedAt = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (notification != null)
        {
            await _dispatcher.DispatchRingNotificationAsync(snapshot.SessionId, notification.Session.HomeId, notification.PressCount, mediaUrl);
            _logger.LogInformation("Notification for session {SessionId} re-dispatched with visitor photo {MediaUrl}", snapshot.SessionId, mediaUrl);
        }
    }

    private async Task TriggerChimeCommandAsync(Guid homeId, Guid ringEventId, CancellationToken cancellationToken)
    {
        var chimeDevice = await _unitOfWork.Devices.GetByHomeAndTypeAsync(homeId, DeviceType.Chime, cancellationToken);
        var melody = await _unitOfWork.ChimeMelodies.GetMainMelodyByHomeIdAsync(homeId, cancellationToken);

        if (chimeDevice != null)
        {
            var melodyId = melody?.Id ?? Guid.Empty;
            string? toneFileUrl = null;

            if (melody != null && !string.IsNullOrWhiteSpace(melody.ToneFileUrl))
            {
                var rawUrl = melody.ToneFileUrl;
                string objectKey;

                if (Uri.TryCreate(rawUrl, UriKind.Absolute, out var uri))
                {
                    var path = Uri.UnescapeDataString(uri.AbsolutePath).TrimStart('/');
                    const string bucketPrefix = "doorbell-melodies/";
                    if (path.StartsWith(bucketPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        objectKey = path.Substring(bucketPrefix.Length);
                    }
                    else
                    {
                        objectKey = path;
                    }
                }
                else
                {
                    objectKey = rawUrl.TrimStart('/');
                    const string bucketPrefix = "doorbell-melodies/";
                    if (objectKey.StartsWith(bucketPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        objectKey = objectKey.Substring(bucketPrefix.Length);
                    }
                }


                while (objectKey.StartsWith("melodies/melodies/", StringComparison.OrdinalIgnoreCase))
                {
                    objectKey = objectKey.Substring(9); 
                }

                toneFileUrl = await _minioService.GetPresignedObjectUrlAsync(objectKey, "doorbell-melodies", cancellationToken);

                var command = new ChimeCommand
                {
                    Id = Guid.NewGuid(),
                    RingEventId = ringEventId,
                    DeviceId = chimeDevice.Id,
                    MelodyId = melody.Id,
                    SentAt = DateTime.UtcNow,
                    AckStatus = AckStatus.Pending
                };
                await _unitOfWork.ChimeCommands.AddAsync(command, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            await _mqttService.PublishChimeCommandAsync(chimeDevice.Id, melodyId, ringEventId, toneFileUrl, cancellationToken);
        }
    }

    private async Task TriggerCameraSnapshotCommandAsync(Guid homeId, Guid sessionId, CancellationToken cancellationToken)
    {
        var cameraDevice = await _unitOfWork.Devices.GetByHomeAndTypeAsync(homeId, DeviceType.Camera, cancellationToken);
        var bucket = _configuration["MinioOptions:BucketName"] ?? "doorbell-snapshots";

        if (cameraDevice != null)
        {
            var objectKey = $"snapshots/{homeId}/{sessionId}.jpg";
            var snapshot = new CameraSnapshot
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                DeviceId = cameraDevice.Id,
                Bucket = bucket,
                ObjectKey = objectKey,
                SentAt = DateTime.UtcNow,
                AckStatus = AckStatus.Pending
            };

            await _unitOfWork.CameraSnapshots.AddAsync(snapshot, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _mqttService.PublishCameraSnapshotCommandAsync(cameraDevice.Id, sessionId, objectKey, cancellationToken);
        }
    }

    private Task StartSessionTimersAsync(Guid buttonDeviceId, Guid sessionId)
    {
        var silenceSeconds = SilenceTimeoutSeconds;
        var escalationSeconds = EscalationTimeoutSeconds;

        _ = Task.Run(async () =>
        {
            try
            {

                await Task.Delay(TimeSpan.FromSeconds(escalationSeconds));

                using var scope = _scopeFactory.CreateScope();
                var redisCache = scope.ServiceProvider.GetRequiredService<IRedisSessionCache>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var dispatcher = scope.ServiceProvider.GetRequiredService<INotificationDispatcher>();

                var activeSessionId = await redisCache.GetActiveSessionIdAsync(buttonDeviceId);
                if (activeSessionId == sessionId)
                {
                    var session = await unitOfWork.RingSessions.GetByIdAsync(sessionId);
                    if (session != null && session.ClosedAt == null)
                    {
                        session.EscalatedAt = DateTime.UtcNow;
                        await unitOfWork.SaveChangesAsync();
                        _logger.LogInformation("Escalation timer fired for session {SessionId}.", sessionId);
                        await dispatcher.DispatchRingNotificationAsync(sessionId, session.HomeId, session.PressCount, session.Notification?.ImageUrl);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in escalation timer loop for session {SessionId}", sessionId);
            }
        });

        _ = Task.Run(async () =>
        {
            try
            {

                while (true)
                {
                    await Task.Delay(TimeSpan.FromSeconds(silenceSeconds));

                    using var scope = _scopeFactory.CreateScope();
                    var redisCache = scope.ServiceProvider.GetRequiredService<IRedisSessionCache>();
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var doorbellService = scope.ServiceProvider.GetRequiredService<IDoorbellService>();

                    var activeSessionId = await redisCache.GetActiveSessionIdAsync(buttonDeviceId);
                    if (activeSessionId != sessionId)
                    {

                        await doorbellService.CloseSessionAsync(sessionId);
                        break;
                    }

                    var session = await unitOfWork.RingSessions.GetByIdAsync(sessionId);
                    if (session == null || session.ClosedAt != null)
                    {
                        break;
                    }

                    var elapsed = DateTime.UtcNow - session.LastPressAt;
                    if (elapsed.TotalSeconds >= silenceSeconds)
                    {
                        await doorbellService.CloseSessionAsync(sessionId);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in silence window monitoring loop for session {SessionId}", sessionId);
            }
        });

        return Task.CompletedTask;
    }
}
