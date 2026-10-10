namespace smart_doorbell.Application.DTOs;

public record SubscribePushDto(
    string Endpoint,
    string P256dhKey,
    string AuthKey
);

public record PushSubscriptionResponseDto(
    Guid Id,
    Guid UserId,
    string Endpoint,
    DateTime CreatedAt,
    DateTime? LastUsedAt
);
