namespace smart_doorbell.Domain.Entities;

public class RingEvent
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid DeviceId { get; set; }
    public string DeviceEventId { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

    public RingSession Session { get; set; } = null!;
    public Device Device { get; set; } = null!;
    public ICollection<ChimeCommand> ChimeCommands { get; set; } = new List<ChimeCommand>();
}
