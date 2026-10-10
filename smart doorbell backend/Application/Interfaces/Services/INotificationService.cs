using smart_doorbell.Application.DTOs;

namespace smart_doorbell.Application.Interfaces.Services;

public interface INotificationService
{
    Task<NotificationDetailDto?> GetNotificationAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task<RingSessionDetailDto?> GetSessionDetailsAsync(Guid sessionId, CancellationToken cancellationToken = default);
}
