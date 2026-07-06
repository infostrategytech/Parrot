namespace Parrot.Application.Integrations.BusinessEnrichment;

public class BusinessEnrichmentRequest
{
    public string Website { get; set; } = string.Empty;
    public string Facebook { get; set; } = string.Empty;
    public string Instagram { get; set; } = string.Empty;
    public string TwitterX { get; set; } = string.Empty;
    public string LinkedIn { get; set; } = string.Empty;
    public string YouTube { get; set; } = string.Empty;
    public string TikTok { get; set; } = string.Empty;
}

public class BusinessEnrichmentResult
{
    public string Description { get; set; } = string.Empty;
    public string SuggestedIndustry { get; set; } = string.Empty;
    public string SuggestedSize { get; set; } = string.Empty;
    public string BrandVoice { get; set; } = string.Empty;
    public List<string> KeyFacts { get; set; } = new();
    public List<string> SourcesUsed { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
