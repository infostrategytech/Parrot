namespace Parrot.Web.Components.Pages.Dashboard.Team;

public class TeamMember
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class RolePermission
{
    public string Label { get; set; } = string.Empty;
    public bool Enabled { get; set; }
}

public static class WorkspaceRoles
{
    public const string ConversationManager = "Conversation Manager";
    public const string AutomationManager = "Automation Manager";
    public const string ChannelManager = "Channel Manager";
    public const string CatalogManager = "Catalog Manager";
    public const string Analyst = "Analyst";

    public static readonly string[] All =
    [
        ConversationManager,
        AutomationManager,
        ChannelManager,
        CatalogManager,
        Analyst
    ];

    public static List<RolePermission> GetPermissions(string role) => role switch
    {
        ConversationManager =>
        [
            new() { Label = "View conversations" },
            new() { Label = "Take control of conversation" },
            new() { Label = "Send manual replies" },
            new() { Label = "Close conversation" },
            new() { Label = "Escalate conversation" },
            new() { Label = "View conversation logs" }
        ],
        AutomationManager =>
        [
            new() { Label = "Create playbook" },
            new() { Label = "Edit playbook" },
            new() { Label = "Delete playbook" },
            new() { Label = "Publish playbook" },
            new() { Label = "Create AI agent" },
            new() { Label = "Edit AI agent" }
        ],
        ChannelManager =>
        [
            new() { Label = "Connect channel" },
            new() { Label = "Disconnect channel" }
        ],
        CatalogManager =>
        [
            new() { Label = "View catalog" },
            new() { Label = "Manage categories" },
            new() { Label = "Add items" },
            new() { Label = "Edit items" },
            new() { Label = "Delete item" },
            new() { Label = "Update availability" }
        ],
        Analyst =>
        [
            new() { Label = "View dashboard metrics" },
            new() { Label = "View QA analysis" },
            new() { Label = "View conversation summaries" },
            new() { Label = "View task reports" }
        ],
        _ => []
    };
}
