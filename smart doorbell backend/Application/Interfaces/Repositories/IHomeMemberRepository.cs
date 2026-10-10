using smart_doorbell.Domain.Entities;

namespace smart_doorbell.Application.Interfaces.Repositories;

public interface IHomeMemberRepository
{
    Task<bool> IsMemberAsync(Guid homeId, Guid userId, CancellationToken cancellationToken = default);
    Task<List<Guid>> GetMemberUserIdsAsync(Guid homeId, CancellationToken cancellationToken = default);
    Task<List<HomeMember>> GetMembersWithUserByHomeIdAsync(Guid homeId, CancellationToken cancellationToken = default);
    Task<HomeMember?> GetMemberAsync(Guid homeId, Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(HomeMember member, CancellationToken cancellationToken = default);
    void Delete(HomeMember member);
}
