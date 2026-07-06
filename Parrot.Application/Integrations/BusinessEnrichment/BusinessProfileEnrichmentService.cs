using System.Text.Json;
using System.Text.Json.Serialization;
using Anthropic;
using Anthropic.Models.Messages;
using Microsoft.Extensions.Options;
using Parrot.Application.Logging;

namespace Parrot.Application.Integrations.BusinessEnrichment;

public class BusinessProfileEnrichmentService : IBusinessProfileEnrichmentService
{
    private const int MaxContentCharsPerSource = 6000;

    private readonly IBrightDataScraperService _scraper;
    private readonly AnthropicClient _anthropicClient;
    private readonly AnthropicSettings _settings;
    private readonly IAppLogger<BusinessProfileEnrichmentService> _logger;

    public BusinessProfileEnrichmentService(
        IBrightDataScraperService scraper,
        AnthropicClient anthropicClient,
        IOptions<AnthropicSettings> settings,
        IAppLogger<BusinessProfileEnrichmentService> logger)
    {
        _scraper = scraper;
        _anthropicClient = anthropicClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<BusinessEnrichmentResult> EnrichAsync(BusinessEnrichmentRequest request, CancellationToken cancellationToken = default)
    {
        List<string> sourceUrls = BuildSourceUrls(request);
        BusinessEnrichmentResult result = new();

        if (sourceUrls.Count == 0)
        {
            result.Warnings.Add("No website or social media handles were provided, so no context could be pulled.");
            return result;
        }

        ScrapedPage[] pages = await Task.WhenAll(sourceUrls.Select(url => _scraper.FetchAsync(url, cancellationToken)));

        List<ScrapedPage> succeeded = pages.Where(p => p.Success && !string.IsNullOrWhiteSpace(p.Content)).ToList();
        result.SourcesUsed.AddRange(succeeded.Select(p => p.Url));
        result.Warnings.AddRange(pages.Where(p => !p.Success).Select(p => $"Could not fetch {p.Url}{(p.Error is null ? string.Empty : $" ({p.Error})")}."));

        if (succeeded.Count == 0)
        {
            result.Warnings.Add("None of the provided website/social links could be fetched.");
            return result;
        }

        string prompt = BuildPrompt(succeeded);
        SynthesizedProfile? synthesized = await SynthesizeAsync(prompt, cancellationToken);

        if (synthesized is null)
        {
            result.Warnings.Add("The AI enrichment step did not return usable content — you can still fill this in manually.");
            return result;
        }

        result.Description = synthesized.Description;
        result.SuggestedIndustry = synthesized.SuggestedIndustry;
        result.SuggestedSize = synthesized.SuggestedSize;
        result.BrandVoice = synthesized.BrandVoice;
        result.KeyFacts.AddRange(synthesized.KeyFacts);
        return result;
    }

    private static List<string> BuildSourceUrls(BusinessEnrichmentRequest request)
    {
        List<string> urls = new();

        if (!string.IsNullOrWhiteSpace(request.Website))
        {
            string website = request.Website.Trim();
            urls.Add(website.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? website : $"https://{website}");
        }

        AddHandleUrl(urls, request.Facebook, handle => $"https://facebook.com/{handle}");
        AddHandleUrl(urls, request.Instagram, handle => $"https://instagram.com/{handle}");
        AddHandleUrl(urls, request.TwitterX, handle => $"https://x.com/{handle}");
        AddHandleUrl(urls, request.LinkedIn, handle => $"https://linkedin.com/company/{handle}");
        AddHandleUrl(urls, request.YouTube, handle => $"https://youtube.com/@{handle}");
        AddHandleUrl(urls, request.TikTok, handle => $"https://tiktok.com/@{handle}");

        return urls;
    }

    private static void AddHandleUrl(List<string> urls, string handle, Func<string, string> buildUrl)
    {
        if (!string.IsNullOrWhiteSpace(handle))
        {
            urls.Add(buildUrl(handle.Trim().TrimStart('@')));
        }
    }

    private static string BuildPrompt(List<ScrapedPage> pages)
    {
        System.Text.StringBuilder sb = new();
        sb.AppendLine("You are helping populate a business's workspace profile from publicly available content.");
        sb.AppendLine("Below is content fetched from the business's own website and/or social media profiles.");
        sb.AppendLine("Summarize what you can confidently infer. If something isn't evident from the content, leave it blank rather than guessing.");
        sb.AppendLine();

        foreach (ScrapedPage page in pages)
        {
            string content = page.Content.Length > MaxContentCharsPerSource
                ? page.Content[..MaxContentCharsPerSource]
                : page.Content;
            sb.AppendLine($"--- Source: {page.Url} ---");
            sb.AppendLine(content);
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private async Task<SynthesizedProfile?> SynthesizeAsync(string prompt, CancellationToken cancellationToken)
    {
        Dictionary<string, JsonElement> schema = new()
        {
            ["type"] = JsonSerializer.SerializeToElement("object"),
            ["properties"] = JsonSerializer.SerializeToElement(new
            {
                description = new { type = "string", description = "A 2-3 sentence overview of what the business does and who it serves." },
                suggested_industry = new { type = "string", description = "Best-guess industry category, or empty string if unclear." },
                suggested_size = new { type = "string", description = "Best-guess business size (e.g. 'Solo', '2-10 employees'), or empty string if unclear." },
                brand_voice = new { type = "string", description = "A short phrase describing the brand's tone/voice (e.g. 'friendly and casual'), or empty string if unclear." },
                key_facts = new
                {
                    type = "array",
                    items = new { type = "string" },
                    description = "3-6 short, concrete facts about the business (products, location, notable achievements).",
                },
            }),
            ["required"] = JsonSerializer.SerializeToElement(new[] { "description", "suggested_industry", "suggested_size", "brand_voice", "key_facts" }),
            ["additionalProperties"] = JsonSerializer.SerializeToElement(false),
        };

        OutputConfig outputConfig = new()
        {
            Effort = Effort.Medium,
            Format = new JsonOutputFormat { Schema = schema },
        };
        List<MessageParam> messages = [new() { Role = Role.User, Content = prompt }];

        Message response = await _anthropicClient.Messages.Create(
            new MessageCreateParams { Model = _settings.Model, MaxTokens = 4000, OutputConfig = outputConfig, Messages = messages },
            cancellationToken);

        if (response.StopReason == "refusal")
        {
            _logger.LogWarning("Business enrichment request was refused by {Model}; retrying with {FallbackModel}", _settings.Model, _settings.FallbackModel);
            response = await _anthropicClient.Messages.Create(
                new MessageCreateParams { Model = _settings.FallbackModel, MaxTokens = 4000, OutputConfig = outputConfig, Messages = messages },
                cancellationToken);

            if (response.StopReason == "refusal")
            {
                _logger.LogWarning("Business enrichment request was refused by fallback model {FallbackModel} as well", _settings.FallbackModel);
                return null;
            }
        }

        TextBlock? textBlock = response.Content.Select(b => b.Value).OfType<TextBlock>().FirstOrDefault();
        if (textBlock is null)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<SynthesizedProfile>(textBlock.Text, JsonSerializerOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse business enrichment JSON output");
            return null;
        }
    }

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    private class SynthesizedProfile
    {
        public string Description { get; set; } = string.Empty;

        public string SuggestedIndustry { get; set; } = string.Empty;

        public string SuggestedSize { get; set; } = string.Empty;

        public string BrandVoice { get; set; } = string.Empty;

        public List<string> KeyFacts { get; set; } = new();
    }
}
