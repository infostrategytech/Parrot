namespace Parrot.Web.Features.Dashboard.Team.State
{
    public class TeamState
    {
        public bool IsLoading { get; private set; }

        public void SetLoading(bool value) => IsLoading = value;
    }
}
