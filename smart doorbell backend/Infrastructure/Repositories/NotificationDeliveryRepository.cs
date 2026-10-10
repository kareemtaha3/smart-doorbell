using Microsoft.EntityFrameworkCore;
using smart_doorbell.Application.Interfaces.Repositories;
using smart_doorbell.Domain.Entities;
using smart_doorbell.Domain.Enums;
using smart_doorbell.Infrastructure.Data;

namespace smart_doorbell.Infrastructure.Repositories;

public class NotificationDeliveryRepository : INotificationDeliveryRepository
{
    private readonly AppDbContext _db;

    public NotificationDeliveryRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<NotificationDelivery?> GetByNotificationUserAndChannelAsync(Guid notificationId, Guid userId, NotificationChannel channel, CancellationToken cancellationToken = default)
    {
        return await _db.NotificationDeliveries
            .FirstOrDefaultAsync(d => d.NotificationId == notificationId && d.UserId == userId && d.Channel == channel, cancellationToken);
    }

    public async Task AddAsync(NotificationDelivery delivery, CancellationToken cancellationToken = default)
    {
        await _db.NotificationDeliveries.AddAsync(delivery, cancellationToken);
    }
}
