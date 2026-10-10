namespace smart_doorbell.Application.DTOs;

public record CreateHomeDto(
    string Name,
    Guid OwnerUserId
);

public record UpdateHomeDto(
    string Name
);

public record HomeMemberResponseDto(
    Guid Id,
    Guid UserId,
    string Username,
    string DisplayName,
    string Role,
    DateTime CreatedAt
);

public record DeviceResponseDto(
    Guid Id,
    string Type,
    string Status,
    DateTime? LastSeenAt
);

public record HomeResponseDto(
    Guid Id,
    string Name,
    Guid OwnerUserId,
    DateTime CreatedAt,
    List<HomeMemberResponseDto> Members,
    List<DeviceResponseDto> Devices
);
