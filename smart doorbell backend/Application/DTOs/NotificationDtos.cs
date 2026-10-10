using smart_doorbell.Domain.Enums;

namespace smart_doorbell.Application.DTOs;

public record NotificationDeliveryDto(
    Guid Id,
    Guid UserId,
    NotificationChannel Channel,
    DateTime SentAt,
    DeliveryStatus Status,
    DateTime? ReadAt
);

public record NotificationDetailDto(
    Guid Id,
    Guid SessionId,
    Guid HomeId,
    string Title,
    string Body,
    int PressCount,
    string? ImageUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<NotificationDeliveryDto> Deliveries
);

public record RingSessionDetailDto(
    Guid SessionId,
    Guid ButtonDeviceId,
    Guid HomeId,
    DateTime CreatedAt,
    DateTime LastPressAt,
    DateTime? ClosedAt,
    int PressCount,
    DateTime? EscalatedAt,
    NotificationDetailDto? Notification
);
