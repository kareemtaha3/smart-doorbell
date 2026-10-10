namespace smart_doorbell.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Home> OwnedHomes { get; set; } = new List<Home>();
    public ICollection<HomeMember> HomeMemberships { get; set; } = new List<HomeMember>();
    public ICollection<HomeInvitation> SentInvitations { get; set; } = new List<HomeInvitation>();
    public ICollection<HomeInvitation> ReceivedInvitations { get; set; } = new List<HomeInvitation>();
    public ICollection<PushSubscription> PushSubscriptions { get; set; } = new List<PushSubscription>();
    public ICollection<NotificationDelivery> NotificationDeliveries { get; set; } = new List<NotificationDelivery>();
}
