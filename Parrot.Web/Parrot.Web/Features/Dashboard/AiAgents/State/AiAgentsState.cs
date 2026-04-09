namespace Parrot.Web.Features.Dashboard.AiAgents.State
{
    public class AiAgentsState
    {
        public bool IsLoading { get; private set; }

        public void SetLoading(bool value) => IsLoading = value;
    }
}
