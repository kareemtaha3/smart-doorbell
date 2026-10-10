using smart_doorbell.Domain.Entities;
using smart_doorbell.Domain.Enums;

namespace smart_doorbell.Application.Interfaces.Repositories;

public interface INotificationDeliveryRepository
{
    Task<NotificationDelivery?> GetByNotificationUserAndChannelAsync(Guid notificationId, Guid userId, NotificationChannel channel, CancellationToken cancellationToken = default);
    Task AddAsync(NotificationDelivery delivery, CancellationToken cancellationToken = default);
}
