namespace Parrot.Web.Features.Dashboard.TaskRequests.State
{
    public class TaskRequestsState
    {
        public bool IsLoading { get; private set; }

        public void SetLoading(bool value) => IsLoading = value;
    }
}
