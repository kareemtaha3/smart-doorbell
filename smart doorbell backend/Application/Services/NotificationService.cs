using smart_doorbell.Application.DTOs;
using smart_doorbell.Application.Interfaces.Repositories;
using smart_doorbell.Application.Interfaces.Services;

namespace smart_doorbell.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;

    public NotificationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<NotificationDetailDto?> GetNotificationAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId, cancellationToken);
        if (notification == null) return null;

        var deliveries = notification.Deliveries.Select(d => new NotificationDeliveryDto(
            d.Id,
            d.UserId,
            d.Channel,
            d.SentAt,
            d.Status,
            d.ReadAt
        )).ToList();

        return new NotificationDetailDto(
            notification.Id,
            notification.SessionId,
            notification.Session.HomeId,
            notification.Title,
            notification.Body,
            notification.PressCount,
            notification.ImageUrl,
            notification.CreatedAt,
            notification.UpdatedAt,
            deliveries
        );
    }

    public async Task<RingSessionDetailDto?> GetSessionDetailsAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _unitOfWork.RingSessions.GetByIdAsync(sessionId, cancellationToken);
        if (session == null) return null;

        NotificationDetailDto? notificationDto = null;
        if (session.Notification != null)
        {
            var deliveries = session.Notification.Deliveries.Select(d => new NotificationDeliveryDto(
                d.Id,
                d.UserId,
                d.Channel,
                d.SentAt,
                d.Status,
                d.ReadAt
            )).ToList();

            notificationDto = new NotificationDetailDto(
                session.Notification.Id,
                session.Notification.SessionId,
                session.HomeId,
                session.Notification.Title,
                session.Notification.Body,
                session.Notification.PressCount,
                session.Notification.ImageUrl,
                session.Notification.CreatedAt,
                session.Notification.UpdatedAt,
                deliveries
            );
        }

        return new RingSessionDetailDto(
            session.Id,
            session.ButtonDeviceId,
            session.HomeId,
            session.CreatedAt,
            session.LastPressAt,
            session.ClosedAt,
            session.PressCount,
            session.EscalatedAt,
            notificationDto
        );
    }
}
