using System.Net.Http.Headers;
using System.Net.Http.Json;
using Parrot.Web.Features.Dashboard.Channels.Models;

namespace Parrot.Web.Features.Dashboard.Channels.Services;

public class IntegrationsService : IIntegrationsService
{
    private readonly HttpClient _httpClient;

    public IntegrationsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string?> GetWebhookUrlAsync(string platform, string token, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/integrations/business-webhook/{platform}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;
        WebhookUrlResponse? result = await response.Content.ReadFromJsonAsync<WebhookUrlResponse>(cancellationToken);
        return result?.WebhookUrl;
    }
}
