namespace Parrot.Web.Components.Pages.Dashboard.TaskRequests;

public class TaskActivityLog
{
    public string Timestamp { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class TaskMessage
{
    public string Text { get; set; } = string.Empty;
    public bool IsAgent { get; set; }
    public string Time { get; set; } = string.Empty;
}
