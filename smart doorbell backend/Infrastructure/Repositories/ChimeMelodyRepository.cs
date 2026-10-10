using Microsoft.EntityFrameworkCore;
using smart_doorbell.Application.Interfaces.Repositories;
using smart_doorbell.Domain.Entities;
using smart_doorbell.Infrastructure.Data;

namespace smart_doorbell.Infrastructure.Repositories;

public class ChimeMelodyRepository : IChimeMelodyRepository
{
    private readonly AppDbContext _db;

    public ChimeMelodyRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ChimeMelody?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.ChimeMelodies.FirstOrDefaultAsync(cm => cm.Id == id, cancellationToken);
    }

    public async Task<ChimeMelody?> GetMainMelodyByHomeIdAsync(Guid homeId, CancellationToken cancellationToken = default)
    {
        return await _db.ChimeMelodies.FirstOrDefaultAsync(cm => cm.HomeId == homeId && cm.IsMain, cancellationToken);
    }

    public async Task<List<ChimeMelody>> GetByHomeIdAsync(Guid homeId, CancellationToken cancellationToken = default)
    {
        return await _db.ChimeMelodies
            .Where(cm => cm.HomeId == homeId)
            .OrderByDescending(cm => cm.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task UnsetMainMelodiesForHomeAsync(Guid homeId, CancellationToken cancellationToken = default)
    {
        var mainMelodies = await _db.ChimeMelodies
            .Where(cm => cm.HomeId == homeId && cm.IsMain)
            .ToListAsync(cancellationToken);

        foreach (var melody in mainMelodies)
        {
            melody.IsMain = false;
        }
    }

    public async Task AddAsync(ChimeMelody melody, CancellationToken cancellationToken = default)
    {
        await _db.ChimeMelodies.AddAsync(melody, cancellationToken);
    }
}
