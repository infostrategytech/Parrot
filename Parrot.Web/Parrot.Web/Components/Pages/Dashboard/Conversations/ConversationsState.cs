namespace Parrot.Web.Components.Pages.Dashboard.Conversations;

public class ConversationsState
{
    public List<ConversationItem> Conversations { get; } = new()
        {
            new ConversationItem
            {
                Id = "conv-1",
                CustomerName = "Michael Wong",
                CustomerAvatar = "assets/images/avatars/avatar-1.png",
                PrimaryChannel = "instagram",
                AgentName = "Bakerbot",
                Status = "Open",
                LastMessage = "Great, thanks for the info! I will order i...",
                TimeAgo = "2 mins ago",
                Tags = new() { "VIP", "Frequent buyer" },
                OrderCount = 24,
                AiHandling = "Bakerbot",
                DetectedSteps = new() { "Ask for price", "Place order" },
                Channels = new()
                {
                    new ConvChannel { Key = "whatsapp", Name = "WhatsApp Business", Handle = "+2348012345678" },
                    new ConvChannel { Key = "instagram", Name = "Instagram", Handle = "@john_doe" }
                },
                Messages = new()
                {
                    new ConvMessage { Text = "2 plates of Jollof Rice with chicken and a Pepsi", IsAi = false },
                    new ConvMessage { Text = "Got it! So that's:\n• Jollof Rice x2\n• Chicken x1\n• Pepsi x1\nWould you like anything else?", IsAi = true },
                    new ConvMessage { Text = "No, that's all. Make the jollof extra spicy please", IsAi = false },
                    new ConvMessage { Text = "Noted — extra spicy Jollof Rice. Your total comes to ₦7,000. Should I go ahead and place this order?", IsAi = true },
                    new ConvMessage { Text = "Yes please. Deliver to 14 Admiralty Way, Lekki. Gate code is #204. Before 1pm if possible.", IsAi = false, HasTask = true },
                    new ConvMessage { Text = "Perfect! I've placed your order. Here's your summary:\n• Jollof Rice x2 (extra spicy)\n• Chicken x1\n• Pepsi x1\n• Total: ₦7,000\n• Delivery: 14 Admiralty Way, Lekki. You'll receive a confirmation shortly. Thank you!", IsAi = true }
                },
                Tasks = new()
                {
                    new ConvTask { Title = "Task Created", Status = "Pending", TimeAgo = "1hr ago" }
                }
            },
            new ConversationItem
            {
                Id = "conv-2",
                CustomerName = "Guest User",
                CustomerAvatar = "assets/images/avatars/avatar-2.png",
                PrimaryChannel = "whatsapp",
                AgentName = "Bakerbot",
                Status = "Closed",
                LastMessage = "Great, thanks for the info! I will order i...",
                TimeAgo = "2 mins ago",
                Tags = new(),
                OrderCount = 0,
                AiHandling = "Bakerbot",
                DetectedSteps = new(),
                HasDuplicate = true,
                DuplicateName = "Sonia Uzor",
                DuplicateChannel = "instagram",
                Channels = new()
                {
                    new ConvChannel { Key = "whatsapp", Name = "WhatsApp", Handle = "+2348012345678" }
                },
                Messages = new()
                {
                    new ConvMessage { Text = "2 plates of Jollof Rice with chicken and a Pepsi", IsAi = false }
                }
            },
            new ConversationItem
            {
                Id = "conv-3",
                CustomerName = "Kalau Isiagu",
                CustomerAvatar = "assets/images/avatars/avatar-3.png",
                PrimaryChannel = "facebook",
                AgentName = "Bakerbot",
                Status = "Closed",
                LastMessage = "Great, thanks for the info! I will order i...",
                TimeAgo = "2 mins ago",
                Tags = new(),
                OrderCount = 5,
                AiHandling = "Bakerbot",
                DetectedSteps = new() { "Place order" },
                Channels = new()
                {
                    new ConvChannel { Key = "facebook", Name = "Facebook", Handle = "@kalau.isiagu" }
                },
                Messages = new()
                {
                    new ConvMessage { Text = "I'd like to place an order", IsAi = false },
                    new ConvMessage { Text = "Sure! What would you like to order?", IsAi = true }
                }
            },
            new ConversationItem
            {
                Id = "conv-4",
                CustomerName = "Isong Benedict",
                CustomerAvatar = "assets/images/avatars/avatar-4.png",
                PrimaryChannel = "facebook",
                AgentName = "Bakerbot",
                Status = "Closed",
                LastMessage = "Great, thanks for the info! I will order i...",
                TimeAgo = "2 mins ago",
                Tags = new(),
                OrderCount = 2,
                AiHandling = "Bakerbot",
                DetectedSteps = new(),
                Channels = new()
                {
                    new ConvChannel { Key = "facebook", Name = "Facebook", Handle = "@isong.ben" }
                },
                Messages = new()
                {
                    new ConvMessage { Text = "Hello, do you deliver to Abuja?", IsAi = false },
                    new ConvMessage { Text = "Yes we do! Delivery to Abuja takes 2-3 business days.", IsAi = true }
                }
            },
            new ConversationItem
            {
                Id = "conv-5",
                CustomerName = "Umanah Cynthia",
                CustomerAvatar = "assets/images/avatars/avatar-5.png",
                PrimaryChannel = "instagram",
                AgentName = "Bakerbot",
                Status = "Closed",
                LastMessage = "Great, thanks for the info! I will order i...",
                TimeAgo = "2 mins ago",
                Tags = new(),
                OrderCount = 8,
                AiHandling = "Bakerbot",
                DetectedSteps = new() { "Ask for price" },
                Channels = new()
                {
                    new ConvChannel { Key = "instagram", Name = "Instagram", Handle = "@cynthia_u" }
                },
                Messages = new()
                {
                    new ConvMessage { Text = "How much is the Jollof Rice?", IsAi = false },
                    new ConvMessage { Text = "A plate of Jollof Rice is ₦2,500. Would you like to order?", IsAi = true }
                }
            }
        };

    public static readonly List<ConvSearchResult> SearchResults = new()
        {
            new ConvSearchResult { Id = "r-1", Name = "Sonia Uzor", Channel = "Instagram", Username = "@Mhiz_SoniaU", Avatar = "assets/images/avatars/avatar-4.png" }
        };

    public ConversationItem? GetConversation(string id) =>
        Conversations.FirstOrDefault(c => c.Id == id);
}
