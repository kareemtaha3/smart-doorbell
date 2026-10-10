using smart_doorbell.Domain.Enums;

namespace smart_doorbell.Domain.Entities;

public class NotificationDelivery
{
    public Guid Id { get; set; }
    public Guid NotificationId { get; set; }
    public Guid UserId { get; set; }
    public NotificationChannel Channel { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public DeliveryStatus Status { get; set; } = DeliveryStatus.Sent;
    public DateTime? ReadAt { get; set; }

    public Notification Notification { get; set; } = null!;
    public User User { get; set; } = null!;
}
