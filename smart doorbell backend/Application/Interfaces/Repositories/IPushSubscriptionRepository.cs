using smart_doorbell.Domain.Entities;

namespace smart_doorbell.Application.Interfaces.Repositories;

public interface IPushSubscriptionRepository
{
    Task<PushSubscription?> GetByUserIdAndEndpointAsync(Guid userId, string endpoint, CancellationToken cancellationToken = default);
    Task<List<PushSubscription>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(PushSubscription subscription, CancellationToken cancellationToken = default);
    void Remove(PushSubscription subscription);
}
