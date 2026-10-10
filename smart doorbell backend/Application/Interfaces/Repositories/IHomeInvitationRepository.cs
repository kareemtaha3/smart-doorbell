using smart_doorbell.Domain.Entities;

namespace smart_doorbell.Application.Interfaces.Repositories;

public interface IHomeInvitationRepository
{
    Task<HomeInvitation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<HomeInvitation?> GetByHomeAndInvitedUserAsync(Guid homeId, Guid invitedUserId, CancellationToken cancellationToken = default);
    Task<List<HomeInvitation>> GetByInvitedUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(HomeInvitation invitation, CancellationToken cancellationToken = default);
}
