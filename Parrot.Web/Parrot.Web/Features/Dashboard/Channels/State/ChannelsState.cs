namespace Parrot.Web.Features.Dashboard.Channels.State
{
    public enum ChannelStatus { NotConnected, Connected }

    public class ChannelInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconPath { get; set; } = string.Empty;
        public ChannelStatus Status { get; set; } = ChannelStatus.NotConnected;
        public string? PhoneNumber { get; set; }
        public string? ConnectedDate { get; set; }
        public string Key { get; set; } = string.Empty;
    }

    public class ChannelsState
    {
        public bool IsLoading { get; private set; }

        public List<ChannelInfo> Channels { get; private set; } = new()
        {
            new() { Key = "whatsapp", Name = "Whatsapp Business", Description = "Allow your AI agents to handle customer conversations on WhatsApp.", IconPath = "assets/images/icons/whatsapp.png" },
            new() { Key = "facebook", Name = "Facebook Marketplace", Description = "Handle customer messages from Facebook pages and Marketplace.", IconPath = "assets/images/icons/facebook.png" },
            new() { Key = "instagram", Name = "Instagram", Description = "Automatically respond to customer DMs sent to your Instagram account.", IconPath = "assets/images/icons/instagram.png" },
            new() { Key = "twitter", Name = "X (formerly Twitter)", Description = "Allow your AI agents to handle customer conversations on WhatsApp.", IconPath = "" },
        };

        public void SetLoading(bool value) => IsLoading = value;

        public void ConnectChannel(string key, string phoneNumber, string date)
        {
            var channel = Channels.FirstOrDefault(c => c.Key == key);
            if (channel != null)
            {
                channel.Status = ChannelStatus.Connected;
                channel.PhoneNumber = phoneNumber;
                channel.ConnectedDate = date;
            }
        }

        public void DisconnectChannel(string key)
        {
            var channel = Channels.FirstOrDefault(c => c.Key == key);
            if (channel != null)
            {
                channel.Status = ChannelStatus.NotConnected;
                channel.PhoneNumber = null;
                channel.ConnectedDate = null;
            }
        }
    }
}
