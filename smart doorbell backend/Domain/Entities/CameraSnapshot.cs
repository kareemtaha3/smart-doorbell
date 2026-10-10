using smart_doorbell.Domain.Enums;

namespace smart_doorbell.Domain.Entities;

public class CameraSnapshot
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid DeviceId { get; set; }
    public string Bucket { get; set; } = string.Empty;
    public string ObjectKey { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public AckStatus AckStatus { get; set; } = AckStatus.Pending;
    public DateTime? AckedAt { get; set; }
    public DateTime? ReadyAt { get; set; }
    public string? MediaUrl { get; set; }
    public string? Resolution { get; set; }

    public RingSession Session { get; set; } = null!;
    public Device Device { get; set; } = null!;
}
