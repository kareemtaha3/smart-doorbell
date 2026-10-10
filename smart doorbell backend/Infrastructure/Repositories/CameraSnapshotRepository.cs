using Microsoft.EntityFrameworkCore;
using smart_doorbell.Application.Interfaces.Repositories;
using smart_doorbell.Domain.Entities;
using smart_doorbell.Infrastructure.Data;

namespace smart_doorbell.Infrastructure.Repositories;

public class CameraSnapshotRepository : ICameraSnapshotRepository
{
    private readonly AppDbContext _db;

    public CameraSnapshotRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<CameraSnapshot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.CameraSnapshots.FirstOrDefaultAsync(cs => cs.Id == id, cancellationToken);
    }

    public async Task<CameraSnapshot?> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await _db.CameraSnapshots
            .OrderByDescending(cs => cs.SentAt)
            .FirstOrDefaultAsync(cs => cs.SessionId == sessionId, cancellationToken);
    }

    public async Task<CameraSnapshot?> GetByObjectKeyAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        return await _db.CameraSnapshots.FirstOrDefaultAsync(cs => cs.ObjectKey == objectKey, cancellationToken);
    }

    public async Task AddAsync(CameraSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        await _db.CameraSnapshots.AddAsync(snapshot, cancellationToken);
    }
}
