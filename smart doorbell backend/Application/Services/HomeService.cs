using smart_doorbell.Application.DTOs;
using smart_doorbell.Application.Interfaces.Repositories;
using smart_doorbell.Application.Interfaces.Services;
using smart_doorbell.Domain.Entities;
using smart_doorbell.Domain.Enums;

namespace smart_doorbell.Application.Services;

public class HomeService : IHomeService
{
    private readonly IUnitOfWork _unitOfWork;

    public HomeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<HomeResponseDto> CreateHomeAsync(CreateHomeDto dto, CancellationToken cancellationToken = default)
    {
        var ownerUser = await _unitOfWork.Users.GetByIdAsync(dto.OwnerUserId, cancellationToken);
        if (ownerUser == null)
        {
            throw new KeyNotFoundException($"User with ID '{dto.OwnerUserId}' not found.");
        }

        var home = new Home
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            OwnerUserId = dto.OwnerUserId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Homes.AddAsync(home, cancellationToken);


        var ownerMember = new HomeMember
        {
            Id = Guid.NewGuid(),
            HomeId = home.Id,
            UserId = dto.OwnerUserId,
            Role = HomeRole.Owner,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.HomeMembers.AddAsync(ownerMember, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetHomeByIdAsync(home.Id, cancellationToken)
            ?? throw new InvalidOperationException("Failed to retrieve created home.");
    }

    public async Task<HomeResponseDto?> GetHomeByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var home = await _unitOfWork.Homes.GetByIdWithDetailsAsync(id, cancellationToken);
        if (home == null) return null;

        return MapToResponseDto(home);
    }

    public async Task<List<HomeResponseDto>> GetHomesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var homes = await _unitOfWork.Homes.GetHomesByUserIdAsync(userId, cancellationToken);
        return homes.Select(MapToResponseDto).ToList();
    }

    public async Task<HomeResponseDto> UpdateHomeAsync(Guid id, UpdateHomeDto dto, CancellationToken cancellationToken = default)
    {
        var home = await _unitOfWork.Homes.GetByIdAsync(id, cancellationToken);
        if (home == null)
        {
            throw new KeyNotFoundException($"Home with ID '{id}' not found.");
        }

        home.Name = dto.Name.Trim();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (await GetHomeByIdAsync(id, cancellationToken))!;
    }

    public async Task DeleteHomeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var home = await _unitOfWork.Homes.GetByIdAsync(id, cancellationToken);
        if (home == null)
        {
            throw new KeyNotFoundException($"Home with ID '{id}' not found.");
        }

        _unitOfWork.Homes.Delete(home);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<HomeMemberResponseDto>> GetHomeMembersAsync(Guid homeId, CancellationToken cancellationToken = default)
    {
        var home = await _unitOfWork.Homes.GetByIdAsync(homeId, cancellationToken);
        if (home == null)
        {
            throw new KeyNotFoundException($"Home with ID '{homeId}' not found.");
        }

        var members = await _unitOfWork.HomeMembers.GetMembersWithUserByHomeIdAsync(homeId, cancellationToken);
        return members.Select(m => new HomeMemberResponseDto(
            m.Id,
            m.UserId,
            m.User.Username,
            m.User.DisplayName,
            m.Role.ToString(),
            m.CreatedAt
        )).ToList();
    }

    public async Task RemoveHomeMemberAsync(Guid homeId, Guid userId, CancellationToken cancellationToken = default)
    {
        var member = await _unitOfWork.HomeMembers.GetMemberAsync(homeId, userId, cancellationToken);
        if (member == null)
        {
            throw new KeyNotFoundException($"Member with User ID '{userId}' not found in Home '{homeId}'.");
        }

        if (member.Role == HomeRole.Owner)
        {
            throw new InvalidOperationException("Cannot remove the Owner of the home.");
        }

        _unitOfWork.HomeMembers.Delete(member);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static HomeResponseDto MapToResponseDto(Home home)
    {
        var memberDtos = home.Members.Select(m => new HomeMemberResponseDto(
            m.Id,
            m.UserId,
            m.User?.Username ?? string.Empty,
            m.User?.DisplayName ?? string.Empty,
            m.Role.ToString(),
            m.CreatedAt
        )).ToList();

        var deviceDtos = home.Devices.Select(d => new DeviceResponseDto(
            d.Id,
            d.Type.ToString(),
            d.Status.ToString(),
            d.LastSeenAt
        )).ToList();

        return new HomeResponseDto(
            home.Id,
            home.Name,
            home.OwnerUserId,
            home.CreatedAt,
            memberDtos,
            deviceDtos
        );
    }
}
