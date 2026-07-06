namespace Parrot.Application.Integrations.BusinessEnrichment;

public interface IBrightDataScraperService
{
    Task<ScrapedPage> FetchAsync(string url, CancellationToken cancellationToken = default);
}

public class ScrapedPage
{
    public required string Url { get; init; }
    public bool Success { get; init; }
    public string Content { get; init; } = string.Empty;
    public string? Error { get; init; }
}
