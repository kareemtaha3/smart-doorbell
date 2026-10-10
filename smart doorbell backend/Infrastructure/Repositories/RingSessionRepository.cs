using Microsoft.EntityFrameworkCore;
using smart_doorbell.Application.Interfaces.Repositories;
using smart_doorbell.Domain.Entities;
using smart_doorbell.Infrastructure.Data;

namespace smart_doorbell.Infrastructure.Repositories;

public class RingSessionRepository : IRingSessionRepository
{
    private readonly AppDbContext _db;

    public RingSessionRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<RingSession?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await _db.RingSessions
            .Include(rs => rs.Notification)
            .ThenInclude(n => n!.Deliveries)
            .FirstOrDefaultAsync(rs => rs.Id == sessionId, cancellationToken);
    }

    public async Task AddAsync(RingSession session, CancellationToken cancellationToken = default)
    {
        await _db.RingSessions.AddAsync(session, cancellationToken);
    }
}
