using smart_doorbell.Domain.Enums;

namespace smart_doorbell.Domain.Entities;

public class Device
{
    public Guid Id { get; set; }
    public Guid HomeId { get; set; }
    public DeviceType Type { get; set; }
    public string AuthToken { get; set; } = string.Empty;
    public DeviceStatus Status { get; set; } = DeviceStatus.Offline;
    public DateTime? LastSeenAt { get; set; }

    public Home Home { get; set; } = null!;
    public ICollection<RingSession> RingSessions { get; set; } = new List<RingSession>();
    public ICollection<RingEvent> RingEvents { get; set; } = new List<RingEvent>();
    public ICollection<CameraSnapshot> CameraSnapshots { get; set; } = new List<CameraSnapshot>();
    public ICollection<ChimeCommand> ChimeCommands { get; set; } = new List<ChimeCommand>();
}
