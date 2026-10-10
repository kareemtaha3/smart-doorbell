namespace smart_doorbell.Domain.Entities;

public class ChimeMelody
{
    public Guid Id { get; set; }
    public Guid HomeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ToneFileUrl { get; set; } = string.Empty;
    public bool IsMain { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Home Home { get; set; } = null!;
    public ICollection<ChimeCommand> ChimeCommands { get; set; } = new List<ChimeCommand>();
}
