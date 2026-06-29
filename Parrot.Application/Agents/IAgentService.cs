using Parrot.Application.DTOs.Agents;

namespace Parrot.Application.Agents;

public interface IAgentService
{
    Task<AgentResponse> CreateAsync(string userId, AgentInfo request);

    Task<IReadOnlyList<AgentResponse>> GetAllAsync(string userId);

    Task<AgentResponse> GetByIdAsync(string userId, Guid agentId);
}
