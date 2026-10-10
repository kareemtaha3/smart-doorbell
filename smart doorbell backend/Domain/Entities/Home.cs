namespace smart_doorbell.Domain.Entities;

public class Home
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid OwnerUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User Owner { get; set; } = null!;
    public ICollection<HomeMember> Members { get; set; } = new List<HomeMember>();
    public ICollection<HomeInvitation> Invitations { get; set; } = new List<HomeInvitation>();
    public ICollection<Device> Devices { get; set; } = new List<Device>();
    public ICollection<ChimeMelody> ChimeMelodies { get; set; } = new List<ChimeMelody>();
    public ICollection<RingSession> RingSessions { get; set; } = new List<RingSession>();
}
