namespace smart_doorbell.Domain.Entities;

public class RingSession
{
    public Guid Id { get; set; }
    public Guid ButtonDeviceId { get; set; }
    public Guid HomeId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastPressAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }
    public int PressCount { get; set; } = 1;
    public DateTime? EscalatedAt { get; set; }

    public Device ButtonDevice { get; set; } = null!;
    public Home Home { get; set; } = null!;
    public ICollection<RingEvent> RingEvents { get; set; } = new List<RingEvent>();
    public ICollection<CameraSnapshot> CameraSnapshots { get; set; } = new List<CameraSnapshot>();
    public Notification? Notification { get; set; }
}
