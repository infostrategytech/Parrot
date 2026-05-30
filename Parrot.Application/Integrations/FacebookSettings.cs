namespace Parrot.Application.Integrations;

public class FacebookSettings
{
    public const string SectionName = "Facebook";

    public string WebhookBaseUrl { get; set; } = string.Empty;

    // App-level verify token set when subscribing the webhook in the Meta Developer console
    public string VerifyToken { get; set; } = string.Empty;

    // App Secret — used to validate X-Hub-Signature-256 on every incoming event
    public string AppSecret { get; set; } = string.Empty;
}
