using Parrot.Web.Components.Pages.Dashboard.AiAgents;

namespace Parrot.Web.Features.Dashboard.AiAgents.State
{
    public class AiAgentsState
    {
        public bool IsLoading { get; private set; }
        public List<AiAgent> Agents { get; private set; } = new();

        public void SetLoading(bool value) => IsLoading = value;

        public void AddAgent(AiAgent agent)
        {
            agent.Id = Guid.NewGuid().ToString();
            if (!agent.Channels.Any())
            {
                agent.Channels = new List<AgentChannel>
                {
                    new() { Key = "whatsapp", Name = "WhatsApp Business", Description = "Allow this agent to respond to conversations from this channel.", IsEnabled = false },
                    new() { Key = "facebook", Name = "facebook Marketplace", Description = "Allow this agent to respond to conversations from this channel.", IsEnabled = false },
                    new() { Key = "instagram", Name = "Instagram Dm", Description = "Allow this agent to respond to conversations from this channel.", IsEnabled = false }
                };
            }
            Agents.Add(agent);
        }

        public void RemoveAgent(string id) => Agents.RemoveAll(a => a.Id == id);

        public AiAgent? GetAgent(string id) => Agents.FirstOrDefault(a => a.Id == id);

        public void UpdateAgent(AiAgent updated)
        {
            var idx = Agents.FindIndex(a => a.Id == updated.Id);
            if (idx >= 0)
                Agents[idx] = updated;
        }
    }
}
