using Microsoft.EntityFrameworkCore;
using smart_doorbell.Application.Interfaces.Repositories;
using smart_doorbell.Domain.Entities;
using smart_doorbell.Infrastructure.Data;

namespace smart_doorbell.Infrastructure.Repositories;

public class HomeRepository : IHomeRepository
{
    private readonly AppDbContext _db;

    public HomeRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Home?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Homes.FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public async Task<Home?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Homes
            .Include(h => h.Members)
                .ThenInclude(m => m.User)
            .Include(h => h.Devices)
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public async Task<List<Home>> GetHomesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _db.Homes
            .Include(h => h.Members)
                .ThenInclude(m => m.User)
            .Include(h => h.Devices)
            .Where(h => h.OwnerUserId == userId || h.Members.Any(m => m.UserId == userId))
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<Guid?> GetOwnerUserIdAsync(Guid homeId, CancellationToken cancellationToken = default)
    {
        return await _db.Homes
            .Where(h => h.Id == homeId)
            .Select(h => (Guid?)h.OwnerUserId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(Home home, CancellationToken cancellationToken = default)
    {
        await _db.Homes.AddAsync(home, cancellationToken);
    }

    public void Delete(Home home)
    {
        _db.Homes.Remove(home);
    }
}
