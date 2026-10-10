using smart_doorbell.Domain.Entities;

namespace smart_doorbell.Application.Interfaces.Repositories;

public interface IRingEventRepository
{
    Task AddAsync(RingEvent ringEvent, CancellationToken cancellationToken = default);
}
