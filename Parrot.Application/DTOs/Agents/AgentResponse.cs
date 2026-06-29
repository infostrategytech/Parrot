namespace Parrot.Application.DTOs.Agents;

public class AgentResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MascotUrl { get; set; } = string.Empty;
    public bool SupportsHumanTakeover { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
