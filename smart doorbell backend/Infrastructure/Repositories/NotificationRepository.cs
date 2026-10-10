using Microsoft.EntityFrameworkCore;
using smart_doorbell.Application.Interfaces.Repositories;
using smart_doorbell.Domain.Entities;
using smart_doorbell.Infrastructure.Data;

namespace smart_doorbell.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _db;

    public NotificationRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Notifications
            .Include(n => n.Session)
            .Include(n => n.Deliveries)
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    public async Task<Notification?> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await _db.Notifications
            .Include(n => n.Session)
            .Include(n => n.Deliveries)
            .FirstOrDefaultAsync(n => n.SessionId == sessionId, cancellationToken);
    }

    public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        await _db.Notifications.AddAsync(notification, cancellationToken);
    }
}
