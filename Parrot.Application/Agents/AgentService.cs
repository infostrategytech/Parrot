using Parrot.Application.DTOs.Agents;
using Parrot.Domain.Entities;
using Parrot.Domain.Exceptions;

namespace Parrot.Application.Agents;

public class AgentService : IAgentService
{
    private readonly IAgentRepository _repository;

    public AgentService(IAgentRepository repository)
    {
        _repository = repository;
    }

    public async Task<AgentResponse> CreateAsync(string userId, AgentInfo request)
    {
        Agent agent = new()
        {
            UserId = userId,
            Name = request.Name,
            Description = request.Description,
            MascotUrl = request.MascotUrl,
            SupportsHumanTakeover = request.SupportsHumanTakeover,
        };

        await _repository.AddAsync(agent);
        return ToResponse(agent);
    }

    public async Task<IReadOnlyList<AgentResponse>> GetAllAsync(string userId)
    {
        IReadOnlyList<Agent> agents = await _repository.GetByUserIdAsync(userId);
        return agents.Select(ToResponse).ToList();
    }

    public async Task<AgentResponse> GetByIdAsync(string userId, Guid agentId)
    {
        Agent agent = await _repository.GetByIdAsync(agentId)
            ?? throw new NotFoundException($"Agent {agentId} not found.");

        if (agent.UserId != userId)
            throw new ForbiddenException("You do not have access to this agent.");

        return ToResponse(agent);
    }

    private static AgentResponse ToResponse(Agent agent) => new()
    {
        Id = agent.Id,
        Name = agent.Name,
        Description = agent.Description,
        MascotUrl = agent.MascotUrl,
        SupportsHumanTakeover = agent.SupportsHumanTakeover,
        IsActive = agent.IsActive,
        CreatedAt = agent.CreatedAt,
        UpdatedAt = agent.UpdatedAt,
    };
}
