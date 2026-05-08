namespace Parrot.Web.Components.Pages.Dashboard.TaskRequests;

public class TaskRequest
{
    public string Id { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public string Item { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string ChannelIcon { get; set; } = string.Empty;
    public string AiAgent { get; set; } = string.Empty;
    public string Amount { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string DateCreated { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerContact { get; set; } = string.Empty;
    public List<string> Items { get; set; } = new();
    public List<string> ConversationNotes { get; set; } = new();
    public List<TaskActivityLog> ActivityLog { get; set; } = new();
    public List<TaskMessage> Messages { get; set; } = new();
}
