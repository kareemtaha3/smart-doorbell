using smart_doorbell.Domain.Entities;

namespace smart_doorbell.Application.Interfaces.Repositories;

public interface ICameraSnapshotRepository
{
    Task<CameraSnapshot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CameraSnapshot?> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<CameraSnapshot?> GetByObjectKeyAsync(string objectKey, CancellationToken cancellationToken = default);
    Task AddAsync(CameraSnapshot snapshot, CancellationToken cancellationToken = default);
}
