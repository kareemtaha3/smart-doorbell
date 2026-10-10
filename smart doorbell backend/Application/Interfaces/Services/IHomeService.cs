using smart_doorbell.Application.DTOs;

namespace smart_doorbell.Application.Interfaces.Services;

public interface IHomeService
{
    Task<HomeResponseDto> CreateHomeAsync(CreateHomeDto dto, CancellationToken cancellationToken = default);
    Task<HomeResponseDto?> GetHomeByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<HomeResponseDto>> GetHomesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<HomeResponseDto> UpdateHomeAsync(Guid id, UpdateHomeDto dto, CancellationToken cancellationToken = default);
    Task DeleteHomeAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<HomeMemberResponseDto>> GetHomeMembersAsync(Guid homeId, CancellationToken cancellationToken = default);
    Task RemoveHomeMemberAsync(Guid homeId, Guid userId, CancellationToken cancellationToken = default);
}
