namespace Parrot.Application.Integrations.BusinessEnrichment;

public class AnthropicSettings
{
    public const string SectionName = "Anthropic";

    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "claude-fable-5";
    public string FallbackModel { get; set; } = "claude-opus-4-8";
}
