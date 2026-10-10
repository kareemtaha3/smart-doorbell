using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Protocol;
using smart_doorbell.Application.Interfaces;
using smart_doorbell.Application.Interfaces.Repositories;
using smart_doorbell.Application.Interfaces.Services;
using smart_doorbell.Domain.Enums;

namespace smart_doorbell.Infrastructure.Services;

public class MqttClientService : IMqttClientService, IHostedService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MqttClientService> _logger;
    private readonly IMqttClient _mqttClient;
    private readonly MqttClientOptions _options;

    public MqttClientService(IConfiguration configuration, IServiceScopeFactory scopeFactory, ILogger<MqttClientService> logger)
    {
        _configuration = configuration;
        _scopeFactory = scopeFactory;
        _logger = logger;

        var host = _configuration["MqttOptions:BrokerHost"] ?? "localhost";
        var port = int.TryParse(_configuration["MqttOptions:BrokerPort"], out var p) ? p : 1883;

        var factory = new MqttClientFactory();
        _mqttClient = factory.CreateMqttClient();

        _options = new MqttClientOptionsBuilder()
            .WithTcpServer(host, port)
            .WithClientId($"smart_doorbell_backend_{Guid.NewGuid()}")
            .WithCleanSession()
            .Build();

        _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Connecting Mosquitto MQTT Client to {Host}:{Port}...", _options.ChannelOptions, 1883);
            await _mqttClient.ConnectAsync(_options, cancellationToken);
            _logger.LogInformation("Mosquitto MQTT Client Connected successfully!");

            var topicPrefix = _configuration["MqttOptions:TopicPrefix"] ?? "doorbell";


            await _mqttClient.SubscribeAsync(new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter($"{topicPrefix}/button/+/ring")
                .WithTopicFilter($"{topicPrefix}/chime/+/ack")
                .WithTopicFilter($"{topicPrefix}/camera/+/ack")
                .Build(), cancellationToken);

            _logger.LogInformation("Subscribed to MQTT topics: '{Prefix}/button/+/ring', '{Prefix}/chime/+/ack', '{Prefix}/camera/+/ack'", topicPrefix, topicPrefix, topicPrefix);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not connect to Mosquitto MQTT Broker. Ensure Mosquitto service is running on localhost:1883.");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_mqttClient.IsConnected)
        {
            await _mqttClient.DisconnectAsync(new MqttClientDisconnectOptions(), cancellationToken);
            _logger.LogInformation("Mosquitto MQTT Client Disconnected.");
        }
    }

    public async Task PublishChimeCommandAsync(Guid chimeDeviceId, Guid melodyId, Guid ringEventId, string? toneFileUrl, CancellationToken cancellationToken = default)
    {
        var topicPrefix = _configuration["MqttOptions:TopicPrefix"] ?? "doorbell";
        var topic = $"{topicPrefix}/chime/{chimeDeviceId}/play";

        var payload = JsonSerializer.Serialize(new
        {
            chimeDeviceId,
            melodyId,
            ringEventId,
            toneFileUrl,
            timestamp = DateTime.UtcNow
        });

        if (_mqttClient.IsConnected)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce) // QoS 2
                .Build();

            await _mqttClient.PublishAsync(message, cancellationToken);
            _logger.LogInformation("MQTT [QoS 2] Real Message Published to '{Topic}': {Payload}", topic, payload);
        }
        else
        {
            _logger.LogWarning("MQTT Client disconnected. Command logged locally: {Topic} -> {Payload}", topic, payload);
        }
    }

    public async Task PublishCameraSnapshotCommandAsync(Guid cameraDeviceId, Guid sessionId, string objectKey, CancellationToken cancellationToken = default)
    {
        var topicPrefix = _configuration["MqttOptions:TopicPrefix"] ?? "doorbell";
        var topic = $"{topicPrefix}/camera/{cameraDeviceId}/snapshot";

        var payload = JsonSerializer.Serialize(new
        {
            cameraDeviceId,
            sessionId,
            objectKey,
            bucket = _configuration["MinioOptions:BucketName"] ?? "doorbell-snapshots",
            timestamp = DateTime.UtcNow
        });

        if (_mqttClient.IsConnected)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce) // QoS 2
                .Build();

            await _mqttClient.PublishAsync(message, cancellationToken);
            _logger.LogInformation("MQTT [QoS 2] Real Message Published to '{Topic}': {Payload}", topic, payload);
        }
        else
        {
            _logger.LogWarning("MQTT Client disconnected. Command logged locally: {Topic} -> {Payload}", topic, payload);
        }
    }

    public async Task ProcessChimeAckAsync(Guid chimeDeviceId, Guid ringEventId, CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var chimeCommand = await unitOfWork.ChimeCommands.GetByRingEventIdAsync(ringEventId, cancellationToken);
        if (chimeCommand != null)
        {
            chimeCommand.AckStatus = AckStatus.Acked;
            chimeCommand.AckedAt = DateTime.UtcNow;
            await unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("MQTT [QoS 2] ACK Received from Chime {ChimeDeviceId} for RingEvent {RingEventId}", chimeDeviceId, ringEventId);
        }
    }

    public async Task ProcessCameraAckAsync(Guid cameraDeviceId, Guid sessionId, CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var cameraSnapshot = await unitOfWork.CameraSnapshots.GetBySessionIdAsync(sessionId, cancellationToken);
        if (cameraSnapshot != null)
        {
            cameraSnapshot.AckStatus = AckStatus.Acked;
            cameraSnapshot.AckedAt = DateTime.UtcNow;
            await unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("MQTT [QoS 2] ACK Received from Camera {CameraDeviceId} for Session {SessionId}", cameraDeviceId, sessionId);
        }
    }

    private async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        var topic = args.ApplicationMessage.Topic;
        var payloadStr = args.ApplicationMessage.ConvertPayloadToString();

        _logger.LogInformation("MQTT [QoS 2] Message Received on Topic '{Topic}': {Payload}", topic, payloadStr);

        try
        {
            var jsonNode = JsonNode.Parse(payloadStr);
            var topicPrefix = _configuration["MqttOptions:TopicPrefix"] ?? "doorbell";

            if (topic.EndsWith("/ring"))
            {
                var buttonDeviceIdStr = jsonNode?["buttonDeviceId"]?.ToString() ?? jsonNode?["ButtonDeviceId"]?.ToString();
                var eventId = jsonNode?["deviceEventId"]?.ToString() ?? jsonNode?["DeviceEventId"]?.ToString() ?? $"evt_{DateTime.UtcNow.Ticks}";

                if (Guid.TryParse(buttonDeviceIdStr, out var buttonDeviceId))
                {
                    using var scope = _scopeFactory.CreateScope();
                    var doorbellService = scope.ServiceProvider.GetRequiredService<IDoorbellService>();
                    await doorbellService.ProcessRingEventAsync(buttonDeviceId, eventId);
                }
            }
            else if (topic.EndsWith("/chime/ack") || topic.Contains("/chime/") && topic.EndsWith("/ack"))
            {
                var chimeDeviceIdStr = jsonNode?["chimeDeviceId"]?.ToString() ?? jsonNode?["ChimeDeviceId"]?.ToString();
                var ringEventIdStr = jsonNode?["ringEventId"]?.ToString() ?? jsonNode?["RingEventId"]?.ToString();

                if (Guid.TryParse(chimeDeviceIdStr, out var chimeDeviceId) && Guid.TryParse(ringEventIdStr, out var ringEventId))
                {
                    await ProcessChimeAckAsync(chimeDeviceId, ringEventId);
                }
            }
            else if (topic.EndsWith("/camera/ack") || topic.Contains("/camera/") && topic.EndsWith("/ack"))
            {
                var cameraDeviceIdStr = jsonNode?["cameraDeviceId"]?.ToString() ?? jsonNode?["CameraDeviceId"]?.ToString();
                var sessionIdStr = jsonNode?["sessionId"]?.ToString() ?? jsonNode?["SessionId"]?.ToString();

                if (Guid.TryParse(cameraDeviceIdStr, out var cameraDeviceId) && Guid.TryParse(sessionIdStr, out var sessionId))
                {
                    await ProcessCameraAckAsync(cameraDeviceId, sessionId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing incoming MQTT message on topic '{Topic}'", topic);
        }
    }
}
