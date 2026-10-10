using Microsoft.EntityFrameworkCore;
using smart_doorbell.Application.Interfaces.Repositories;
using smart_doorbell.Domain.Entities;
using smart_doorbell.Infrastructure.Data;

namespace smart_doorbell.Infrastructure.Repositories;

public class ChimeCommandRepository : IChimeCommandRepository
{
    private readonly AppDbContext _db;

    public ChimeCommandRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ChimeCommand?> GetByRingEventIdAsync(Guid ringEventId, CancellationToken cancellationToken = default)
    {
        return await _db.ChimeCommands
            .OrderByDescending(c => c.SentAt)
            .FirstOrDefaultAsync(c => c.RingEventId == ringEventId, cancellationToken);
    }

    public async Task AddAsync(ChimeCommand command, CancellationToken cancellationToken = default)
    {
        await _db.ChimeCommands.AddAsync(command, cancellationToken);
    }
}
