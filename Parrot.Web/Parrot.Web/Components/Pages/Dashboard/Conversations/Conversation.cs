namespace Parrot.Web.Components.Pages.Dashboard.Conversations;

public class ConversationItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerAvatar { get; set; } = "👤";
    public string PrimaryChannel { get; set; } = "whatsapp";
    public string AgentName { get; set; } = "Bakerbot";
    public string Status { get; set; } = "Open";
    public string LastMessage { get; set; } = string.Empty;
    public string TimeAgo { get; set; } = "2 mins ago";
    public List<string> Tags { get; set; } = new();
    public int OrderCount { get; set; }
    public List<ConvChannel> Channels { get; set; } = new();
    public string AiHandling { get; set; } = "Bakerbot";
    public List<string> DetectedSteps { get; set; } = new();
    public List<ConvMessage> Messages { get; set; } = new();
    public bool HasDuplicate { get; set; }
    public string DuplicateName { get; set; } = string.Empty;
    public string DuplicateChannel { get; set; } = "instagram";
    public List<ConvTask> Tasks { get; set; } = new();
}

public class ConvChannel
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Handle { get; set; } = string.Empty;
}

public class ConvMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Text { get; set; } = string.Empty;
    public bool IsAi { get; set; }
    public bool HasTask { get; set; }
}

public class ConvTask
{
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string TimeAgo { get; set; } = "1hr ago";
}

public class ConvSearchResult
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Avatar { get; set; } = "👤";
}
