using smart_doorbell.Domain.Entities;

namespace smart_doorbell.Application.Interfaces.Repositories;

public interface IChimeMelodyRepository
{
    Task<ChimeMelody?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ChimeMelody?> GetMainMelodyByHomeIdAsync(Guid homeId, CancellationToken cancellationToken = default);
    Task<List<ChimeMelody>> GetByHomeIdAsync(Guid homeId, CancellationToken cancellationToken = default);
    Task UnsetMainMelodiesForHomeAsync(Guid homeId, CancellationToken cancellationToken = default);
    Task AddAsync(ChimeMelody melody, CancellationToken cancellationToken = default);
}
