namespace Parrot.Web.Components.Pages.Dashboard.BusinessSetup.Services;

public interface IBusinessSetupService
{
    Task<BusinessEnrichmentResult?> EnrichAsync(BusinessEnrichmentRequest request, string token, CancellationToken cancellationToken = default);
}
