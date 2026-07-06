using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parrot.Application.Integrations.BusinessEnrichment;

namespace Parrot.Api.Rest.Controllers;

[Route("api/business-setup")]
[ApiController]
[Authorize]
public class BusinessSetupController : ControllerBase
{
    private readonly IBusinessProfileEnrichmentService _enrichmentService;

    public BusinessSetupController(IBusinessProfileEnrichmentService enrichmentService)
    {
        _enrichmentService = enrichmentService;
    }

    [HttpPost("enrich")]
    [ProducesResponseType(typeof(BusinessEnrichmentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Enrich([FromBody] BusinessEnrichmentRequest request, CancellationToken cancellationToken)
    {
        BusinessEnrichmentResult result = await _enrichmentService.EnrichAsync(request, cancellationToken);
        return Ok(result);
    }
}
