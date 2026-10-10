using smart_doorbell.Domain.Entities;

namespace smart_doorbell.Application.Interfaces.Repositories;

public interface IRingSessionRepository
{
    Task<RingSession?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task AddAsync(RingSession session, CancellationToken cancellationToken = default);
}
