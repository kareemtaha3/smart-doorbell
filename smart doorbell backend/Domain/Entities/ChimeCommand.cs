using smart_doorbell.Domain.Enums;

namespace smart_doorbell.Domain.Entities;

public class ChimeCommand
{
    public Guid Id { get; set; }
    public Guid RingEventId { get; set; }
    public Guid DeviceId { get; set; }
    public Guid MelodyId { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public AckStatus AckStatus { get; set; } = AckStatus.Pending;
    public DateTime? AckedAt { get; set; }

    public RingEvent RingEvent { get; set; } = null!;
    public Device Device { get; set; } = null!;
    public ChimeMelody Melody { get; set; } = null!;
}
