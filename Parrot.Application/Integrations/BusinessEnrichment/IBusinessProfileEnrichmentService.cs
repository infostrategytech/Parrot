namespace Parrot.Application.Integrations.BusinessEnrichment;

public interface IBusinessProfileEnrichmentService
{
    Task<BusinessEnrichmentResult> EnrichAsync(BusinessEnrichmentRequest request, CancellationToken cancellationToken = default);
}
