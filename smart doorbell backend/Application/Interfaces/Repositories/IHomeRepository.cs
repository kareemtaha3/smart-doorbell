using smart_doorbell.Domain.Entities;

namespace smart_doorbell.Application.Interfaces.Repositories;

public interface IHomeRepository
{
    Task<Home?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Home?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Home>> GetHomesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Guid?> GetOwnerUserIdAsync(Guid homeId, CancellationToken cancellationToken = default);
    Task AddAsync(Home home, CancellationToken cancellationToken = default);
    void Delete(Home home);
}
