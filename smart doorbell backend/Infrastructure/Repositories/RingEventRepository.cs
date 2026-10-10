using smart_doorbell.Application.Interfaces.Repositories;
using smart_doorbell.Domain.Entities;
using smart_doorbell.Infrastructure.Data;

namespace smart_doorbell.Infrastructure.Repositories;

public class RingEventRepository : IRingEventRepository
{
    private readonly AppDbContext _db;

    public RingEventRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(RingEvent ringEvent, CancellationToken cancellationToken = default)
    {
        await _db.RingEvents.AddAsync(ringEvent, cancellationToken);
    }
}
