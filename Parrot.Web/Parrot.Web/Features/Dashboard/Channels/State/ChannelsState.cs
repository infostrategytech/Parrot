namespace Parrot.Web.Features.Dashboard.Channels.State
{
    public class ChannelsState
    {
        public bool IsLoading { get; private set; }

        public void SetLoading(bool value) => IsLoading = value;
    }
}
