using smart_doorbell.Domain.Enums;

namespace smart_doorbell.Application.Interfaces;

public interface INotificationDispatcher
{
    Task DispatchRingNotificationAsync(Guid sessionId, Guid homeId, int pressCount, string? imageUrl);
    Task MarkNotificationAsReadAsync(Guid notificationId, Guid userId, NotificationChannel channel);
}
