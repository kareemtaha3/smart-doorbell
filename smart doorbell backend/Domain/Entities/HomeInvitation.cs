using smart_doorbell.Domain.Enums;

namespace smart_doorbell.Domain.Entities;

public class HomeInvitation
{
    public Guid Id { get; set; }
    public Guid HomeId { get; set; }
    public Guid InvitedByUserId { get; set; }
    public Guid InvitedUserId { get; set; }
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; set; }

    public Home Home { get; set; } = null!;
    public User InvitedByUser { get; set; } = null!;
    public User InvitedUser { get; set; } = null!;
}
