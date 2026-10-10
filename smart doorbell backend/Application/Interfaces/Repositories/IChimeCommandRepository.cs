using smart_doorbell.Domain.Entities;
using smart_doorbell.Domain.Enums;

namespace smart_doorbell.Application.Interfaces.Repositories;

public interface IChimeCommandRepository
{
    Task<ChimeCommand?> GetByRingEventIdAsync(Guid ringEventId, CancellationToken cancellationToken = default);
    Task AddAsync(ChimeCommand command, CancellationToken cancellationToken = default);
}
