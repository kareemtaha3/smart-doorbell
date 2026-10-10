using smart_doorbell.Domain.Enums;

namespace smart_doorbell.Application.DTOs;

public record CreateInvitationDto(
    string InvitedUsername
);

public record InvitationResponseDto(
    Guid Id,
    Guid HomeId,
    string HomeName,
    Guid InvitedByUserId,
    string InvitedByUsername,
    Guid InvitedUserId,
    string InvitedUsername,
    InvitationStatus Status,
    DateTime CreatedAt,
    DateTime? RespondedAt
);

public record RespondInvitationDto(
    bool Accept
);
