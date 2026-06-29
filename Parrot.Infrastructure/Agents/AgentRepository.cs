using Microsoft.EntityFrameworkCore;
using Parrot.Application.Agents;
using Parrot.Domain.Entities;
using Parrot.Infrastructure.Data;

namespace Parrot.Infrastructure.Agents;

public class AgentRepository : IAgentRepository
{
    private readonly ApplicationDbContext _db;

    public AgentRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Agent agent)
    {
        _db.Agents.Add(agent);
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<Agent>> GetByUserIdAsync(string userId) =>
        await _db.Agents
            .Where(a => a.UserId == userId && a.IsActive)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

    public async Task<Agent?> GetByIdAsync(Guid id) =>
        await _db.Agents.FirstOrDefaultAsync(a => a.Id == id);
}
