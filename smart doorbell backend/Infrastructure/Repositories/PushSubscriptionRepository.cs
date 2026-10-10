using Microsoft.EntityFrameworkCore;
using smart_doorbell.Application.Interfaces.Repositories;
using smart_doorbell.Domain.Entities;
using smart_doorbell.Infrastructure.Data;

namespace smart_doorbell.Infrastructure.Repositories;

public class PushSubscriptionRepository : IPushSubscriptionRepository
{
    private readonly AppDbContext _db;

    public PushSubscriptionRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PushSubscription?> GetByUserIdAndEndpointAsync(Guid userId, string endpoint, CancellationToken cancellationToken = default)
    {
        return await _db.PushSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Endpoint == endpoint, cancellationToken);
    }

    public async Task<List<PushSubscription>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _db.PushSubscriptions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(PushSubscription subscription, CancellationToken cancellationToken = default)
    {
        await _db.PushSubscriptions.AddAsync(subscription, cancellationToken);
    }

    public void Remove(PushSubscription subscription)
    {
        _db.PushSubscriptions.Remove(subscription);
    }
}
