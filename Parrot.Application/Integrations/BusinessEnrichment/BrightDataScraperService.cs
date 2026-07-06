using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Parrot.Application.Logging;

namespace Parrot.Application.Integrations.BusinessEnrichment;

public class BrightDataScraperService : IBrightDataScraperService
{
    private readonly HttpClient _httpClient;
    private readonly BrightDataSettings _settings;
    private readonly IAppLogger<BrightDataScraperService> _logger;

    public BrightDataScraperService(
        HttpClient httpClient,
        IOptions<BrightDataSettings> settings,
        IAppLogger<BrightDataScraperService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<ScrapedPage> FetchAsync(string url, CancellationToken cancellationToken = default)
    {
        BrightDataRequest requestBody = new()
        {
            Zone = _settings.Zone,
            Url = url,
            Format = "raw",
            DataFormat = "markdown",
        };

        using HttpRequestMessage request = new(HttpMethod.Post, "request")
        {
            Content = JsonContent.Create(requestBody),
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.ApiKey);

        try
        {
            using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                string body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Bright Data fetch failed for {Url}: {StatusCode} {Body}", url, response.StatusCode, body);
                return new ScrapedPage { Url = url, Success = false, Error = $"HTTP {(int)response.StatusCode}" };
            }

            string content = await response.Content.ReadAsStringAsync(cancellationToken);
            return new ScrapedPage { Url = url, Success = true, Content = content };
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning("Bright Data fetch threw for {Url}: {Message}", url, ex.Message);
            return new ScrapedPage { Url = url, Success = false, Error = ex.Message };
        }
    }

    private class BrightDataRequest
    {
        [JsonPropertyName("zone")]
        public required string Zone { get; init; }

        [JsonPropertyName("url")]
        public required string Url { get; init; }

        [JsonPropertyName("format")]
        public required string Format { get; init; }

        [JsonPropertyName("data_format")]
        public required string DataFormat { get; init; }
    }
}
