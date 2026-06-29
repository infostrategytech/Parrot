namespace Parrot.Domain.Entities;

public class Agent
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string UserId { get; set; }

    public required string Name { get; set; }

    public required string Description { get; set; }

    public required string MascotUrl { get; set; }

    public bool SupportsHumanTakeover { get; set; } = true;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
