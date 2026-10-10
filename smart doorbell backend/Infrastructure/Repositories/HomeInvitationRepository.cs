using Microsoft.EntityFrameworkCore;
using smart_doorbell.Application.Interfaces.Repositories;
using smart_doorbell.Domain.Entities;
using smart_doorbell.Infrastructure.Data;

namespace smart_doorbell.Infrastructure.Repositories;

public class HomeInvitationRepository : IHomeInvitationRepository
{
    private readonly AppDbContext _db;

    public HomeInvitationRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<HomeInvitation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.HomeInvitations
            .Include(i => i.Home)
            .Include(i => i.InvitedByUser)
            .Include(i => i.InvitedUser)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<HomeInvitation?> GetByHomeAndInvitedUserAsync(Guid homeId, Guid invitedUserId, CancellationToken cancellationToken = default)
    {
        return await _db.HomeInvitations
            .FirstOrDefaultAsync(i => i.HomeId == homeId && i.InvitedUserId == invitedUserId, cancellationToken);
    }

    public async Task<List<HomeInvitation>> GetByInvitedUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _db.HomeInvitations
            .Include(i => i.Home)
            .Include(i => i.InvitedByUser)
            .Include(i => i.InvitedUser)
            .Where(i => i.InvitedUserId == userId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(HomeInvitation invitation, CancellationToken cancellationToken = default)
    {
        await _db.HomeInvitations.AddAsync(invitation, cancellationToken);
    }
}
