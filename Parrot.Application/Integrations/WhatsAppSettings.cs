namespace Parrot.Application.Integrations;

public class WhatsAppSettings
{
    public const string SectionName = "WhatsApp";

    public string WebhookBaseUrl { get; set; } = string.Empty;
    public string VerifyToken { get; set; } = string.Empty;
    public string AppSecret { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string PhoneNumberId { get; set; } = string.Empty;
    public string BusinessAccountId { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = "v25.0";
}
