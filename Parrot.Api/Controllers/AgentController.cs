using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parrot.Application.Agents;
using Parrot.Application.DTOs.Agents;

namespace Parrot.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AgentController : ControllerBase
{
    private readonly IAgentService _agentService;

    public AgentController(IAgentService agentService)
    {
        _agentService = agentService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAgent([FromBody] AgentInfo agentInfo)
    {
        string userId = GetUserId();
        AgentResponse agent = await _agentService.CreateAsync(userId, agentInfo);
        return CreatedAtAction(nameof(GetAgent), new { id = agent.Id }, agent);
    }

    [HttpGet]
    public async Task<IActionResult> GetAgents()
    {
        string userId = GetUserId();
        IReadOnlyList<AgentResponse> agents = await _agentService.GetAllAsync(userId);
        return Ok(agents);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAgent(Guid id)
    {
        string userId = GetUserId();
        AgentResponse agent = await _agentService.GetByIdAsync(userId, id);
        return Ok(agent);
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new InvalidOperationException("User ID claim not found.");
}
