using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using smart_doorbell.Application.Interfaces;
using smart_doorbell.Application.Interfaces.Repositories;
using smart_doorbell.Domain.Entities;
using smart_doorbell.Domain.Enums;
using smart_doorbell.Infrastructure.Hubs;

namespace smart_doorbell.Infrastructure.Notifications;

public class NotificationDispatcher : INotificationDispatcher
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISignalRConnectionTracker _connectionTracker;
    private readonly IHubContext<DoorbellHub> _hubContext;
    private readonly IWebPushService _webPushService;
    private readonly ILogger<NotificationDispatcher> _logger;

    public NotificationDispatcher(
        IUnitOfWork unitOfWork,
        ISignalRConnectionTracker connectionTracker,
        IHubContext<DoorbellHub> hubContext,
        IWebPushService webPushService,
        ILogger<NotificationDispatcher> logger)
    {
        _unitOfWork = unitOfWork;
        _connectionTracker = connectionTracker;
        _hubContext = hubContext;
        _webPushService = webPushService;
        _logger = logger;
    }

    public async Task DispatchRingNotificationAsync(Guid sessionId, Guid homeId, int pressCount, string? imageUrl)
    {
        var notification = await _unitOfWork.Notifications.GetBySessionIdAsync(sessionId);

        var title = "Someone is at the door";
        var body = pressCount > 1 ? $"Doorbell pressed {pressCount} times" : "Doorbell pressed";

        if (notification == null)
        {
            notification = new Notification
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                Title = title,
                Body = body,
                PressCount = pressCount,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Notifications.AddAsync(notification);
        }
        else
        {
            notification.Title = title;
            notification.Body = body;
            notification.PressCount = pressCount;
            if (!string.IsNullOrEmpty(imageUrl))
            {
                notification.ImageUrl = imageUrl;
            }
            notification.UpdatedAt = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync();

        var memberUserIds = await _unitOfWork.HomeMembers.GetMemberUserIdsAsync(homeId);
        var homeOwnerId = await _unitOfWork.Homes.GetOwnerUserIdAsync(homeId);

        if (homeOwnerId.HasValue && homeOwnerId.Value != Guid.Empty && !memberUserIds.Contains(homeOwnerId.Value))
        {
            memberUserIds.Add(homeOwnerId.Value);
        }

        var notificationPayload = new
        {
            notificationId = notification.Id,
            sessionId = sessionId,
            homeId = homeId,
            title = notification.Title,
            body = notification.Body,
            pressCount = notification.PressCount,
            imageUrl = notification.ImageUrl,
            tag = sessionId.ToString(),
            timestamp = notification.UpdatedAt
        };

        foreach (var userId in memberUserIds)
        {

            if (_connectionTracker.IsUserOnline(userId))
            {
                var connectionIds = _connectionTracker.GetUserConnections(userId);
                foreach (var connId in connectionIds)
                {
                    await _hubContext.Clients.Client(connId).SendAsync("ReceiveNotification", notificationPayload);
                }

                await UpsertDeliveryRecordAsync(notification.Id, userId, NotificationChannel.SignalR, DeliveryStatus.Sent);
            }


            var pushSubscriptions = await _unitOfWork.PushSubscriptions.GetByUserIdAsync(userId);
            if (pushSubscriptions.Count > 0)
            {
                bool anyPushSent = false;
                foreach (var sub in pushSubscriptions)
                {
                    try
                    {
                        await _webPushService.SendPushNotificationAsync(sub.Endpoint, sub.P256dhKey, sub.AuthKey, notificationPayload);
                        sub.LastUsedAt = DateTime.UtcNow;
                        anyPushSent = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to deliver WebPush to user {UserId} endpoint {Endpoint}", userId, sub.Endpoint);
                    }
                }

                await UpsertDeliveryRecordAsync(
                    notification.Id,
                    userId,
                    NotificationChannel.WebPush,
                    anyPushSent ? DeliveryStatus.Sent : DeliveryStatus.Failed);
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task MarkNotificationAsReadAsync(Guid notificationId, Guid userId, NotificationChannel channel)
    {
        var delivery = await _unitOfWork.NotificationDeliveries
            .GetByNotificationUserAndChannelAsync(notificationId, userId, channel);

        if (delivery != null)
        {
            delivery.ReadAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
        }
    }

    private async Task UpsertDeliveryRecordAsync(Guid notificationId, Guid userId, NotificationChannel channel, DeliveryStatus status)
    {
        var delivery = await _unitOfWork.NotificationDeliveries
            .GetByNotificationUserAndChannelAsync(notificationId, userId, channel);

        if (delivery == null)
        {
            delivery = new NotificationDelivery
            {
                Id = Guid.NewGuid(),
                NotificationId = notificationId,
                UserId = userId,
                Channel = channel,
                SentAt = DateTime.UtcNow,
                Status = status
            };
            await _unitOfWork.NotificationDeliveries.AddAsync(delivery);
        }
        else
        {
            delivery.SentAt = DateTime.UtcNow;
            delivery.Status = status;
        }
    }
}
