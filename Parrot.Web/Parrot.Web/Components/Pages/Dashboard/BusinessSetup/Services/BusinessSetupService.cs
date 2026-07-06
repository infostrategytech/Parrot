using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Parrot.Web.Components.Pages.Dashboard.BusinessSetup.Services;

public class BusinessSetupService : IBusinessSetupService
{
    private readonly HttpClient _httpClient;

    public BusinessSetupService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<BusinessEnrichmentResult?> EnrichAsync(BusinessEnrichmentRequest request, string token, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage httpRequest = new(HttpMethod.Post, "api/business-setup/enrich")
        {
            Content = JsonContent.Create(request),
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<BusinessEnrichmentResult>(cancellationToken);
    }
}
