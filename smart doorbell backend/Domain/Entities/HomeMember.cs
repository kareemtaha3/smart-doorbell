using smart_doorbell.Domain.Enums;

namespace smart_doorbell.Domain.Entities;

public class HomeMember
{
    public Guid Id { get; set; }
    public Guid HomeId { get; set; }
    public Guid UserId { get; set; }
    public HomeRole Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Home Home { get; set; } = null!;
    public User User { get; set; } = null!;
}
