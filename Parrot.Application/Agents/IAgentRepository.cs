using Parrot.Domain.Entities;

namespace Parrot.Application.Agents;

public interface IAgentRepository
{
    Task AddAsync(Agent agent);
    Task<IReadOnlyList<Agent>> GetByUserIdAsync(string userId);
    Task<Agent?> GetByIdAsync(Guid id);
}
