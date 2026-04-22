namespace Parrot.Web.Components.Pages.Dashboard.TaskRequests;

public static class TaskRequestsData
{
    public static readonly List<TaskRequest> All = new()
    {
        new()
        {
            Id = "1",
            RequestId = "TR-001",
            Item = "Jollof rice x 2",
            Channel = "Whatsapp Business",
            ChannelIcon = "whatsapp",
            AiAgent = "BakerBot",
            Amount = "₦7,000",
            Status = "Pending",
            DateCreated = "12 Oct, 2025 • 10:30 AM",
            CustomerName = "Chioma J.",
            CustomerContact = "+234 801 080 25436",
            Items = new() { "Jollof Rice x2", "Chicken", "Pepsi" },
            ConversationNotes = new()
            {
                "Customer requested extra pepper on the Jollof rice.",
                "Delivery preferred before 1:00 PM.",
                "Customer mentioned a gate code: #204.",
            },
            ActivityLog = new()
            {
                new() { Timestamp = "12 Oct 2025, 10:29 AM", Description = "Task created by AI agent \"Main business agent\"\nTriggered after customer confirmed order" },
                new() { Timestamp = "12 Oct 2025, 10:28 AM", Description = "Customer verified order details\nItems and quantity verified" },
                new() { Timestamp = "12 Oct 2025, 10:27 AM", Description = "AI agent detected flow: \"Place food order\"" },
                new() { Timestamp = "12 Oct 2025, 10:25 AM", Description = "Conversation started on whatsapp business" },
            },
            Messages = new()
            {
                new() { Text = "Hi, I want to order food please",                                                   IsAgent = false },
                new() { Text = "Hello! I'd be happy to help you place an order. What would you like?",              IsAgent = true  },
                new() { Text = "2 plates of Jollof Rice with chicken and a Pepsi",                                  IsAgent = false },
                new() { Text = "Got it! So that's: • Jollof Rice x2 • Chicken x1 • Pepsi x1  Would you like anything else?", IsAgent = true },
                new() { Text = "No, that's all. Make the jollof extra spicy please",                                IsAgent = false },
            },
        },
        new()
        {
            Id = "2",
            RequestId = "TR-4591",
            Item = "Chicked & Chips",
            Channel = "Instagram",
            ChannelIcon = "instagram",
            AiAgent = "BakerBot",
            Amount = "₦5,000",
            Status = "Completed",
            DateCreated = "12 Oct, 2025 • 10:30 AM",
            CustomerName = "James O.",
            CustomerContact = "+234 802 000 1234",
            Items = new() { "Chicken & Chips" },
            ConversationNotes = new(),
            ActivityLog = new(),
            Messages = new()
            {
                new() { Text = "Can I get chicken and chips?", IsAgent = false },
                new() { Text = "Sure! One order of Chicken & Chips coming right up.", IsAgent = true },
            },
        },
        new()
        {
            Id = "3",
            RequestId = "TR-4590",
            Item = "Fried rice +2 more",
            Channel = "Facebook Marketplace",
            ChannelIcon = "facebook",
            AiAgent = "Bakerbot",
            Amount = "₦15,000",
            Status = "Cancelled",
            DateCreated = "12 Oct, 2025 • 10:30 AM",
            CustomerName = "Aisha B.",
            CustomerContact = "+234 803 111 5678",
            Items = new() { "Fried Rice", "Chicken x2", "Pepsi x2" },
            ConversationNotes = new(),
            ActivityLog = new(),
            Messages = new()
            {
                new() { Text = "I want fried rice and chicken", IsAgent = false },
                new() { Text = "Got it! Anything else?", IsAgent = true },
                new() { Text = "Add 2 Pepsi please", IsAgent = false },
                new() { Text = "Order cancelled by customer.", IsAgent = true },
            },
        },
    };
}
