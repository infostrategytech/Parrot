namespace Parrot.Web.Features.Dashboard.Conversations.State
{
    public class ConversationsState
    {
        public bool IsLoading { get; private set; }

        public void SetLoading(bool value) => IsLoading = value;
    }
}
