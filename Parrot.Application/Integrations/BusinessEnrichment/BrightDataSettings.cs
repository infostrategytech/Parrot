namespace Parrot.Application.Integrations.BusinessEnrichment;

public class BrightDataSettings
{
    public const string SectionName = "BrightData";

    public string ApiKey { get; set; } = string.Empty;
    public string Zone { get; set; } = string.Empty;
}
