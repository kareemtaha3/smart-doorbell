using Microsoft.EntityFrameworkCore;
using smart_doorbell.Application.Interfaces.Repositories;
using smart_doorbell.Domain.Entities;
using smart_doorbell.Infrastructure.Data;

namespace smart_doorbell.Infrastructure.Repositories;

public class HomeMemberRepository : IHomeMemberRepository
{
    private readonly AppDbContext _db;

    public HomeMemberRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> IsMemberAsync(Guid homeId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _db.HomeMembers.AnyAsync(m => m.HomeId == homeId && m.UserId == userId, cancellationToken);
    }

    public async Task<List<Guid>> GetMemberUserIdsAsync(Guid homeId, CancellationToken cancellationToken = default)
    {
        return await _db.HomeMembers
            .Where(m => m.HomeId == homeId)
            .Select(m => m.UserId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<HomeMember>> GetMembersWithUserByHomeIdAsync(Guid homeId, CancellationToken cancellationToken = default)
    {
        return await _db.HomeMembers
            .Include(m => m.User)
            .Where(m => m.HomeId == homeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<HomeMember?> GetMemberAsync(Guid homeId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _db.HomeMembers
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.HomeId == homeId && m.UserId == userId, cancellationToken);
    }

    public async Task AddAsync(HomeMember member, CancellationToken cancellationToken = default)
    {
        await _db.HomeMembers.AddAsync(member, cancellationToken);
    }

    public void Delete(HomeMember member)
    {
        _db.HomeMembers.Remove(member);
    }
}
