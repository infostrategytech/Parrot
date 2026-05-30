using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Parrot.Api.Utils;
using Parrot.Application.DTOs.Integrations;
using Parrot.Application.Integrations;

namespace Parrot.Api.Rest.Controllers;

[Route("api/webhooks/facebook")]
[ApiController]
public class FacebookWebhookController : ControllerBase
{
    private readonly IFacebookWebhookService _facebookWebhookService;

    public FacebookWebhookController(IFacebookWebhookService facebookWebhookService)
    {
        _facebookWebhookService = facebookWebhookService;
    }

    // Meta calls this GET when the customer subscribes their page to Parrot.
    // We echo back hub.challenge to prove we own the endpoint.
    [HttpGet]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult VerifyWebhook(
        [FromQuery(Name = "hub.mode")] string hubMode,
        [FromQuery(Name = "hub.verify_token")] string hubVerifyToken,
        [FromQuery(Name = "hub.challenge")] string hubChallenge)
    {
        if (!_facebookWebhookService.TryVerifyChallenge(hubMode, hubVerifyToken, hubChallenge, out string echoChallenge))
        {
            return Forbid();
        }

        return Content(echoChallenge, "text/plain");
    }

    // Meta posts all page events here — messages, reactions, deliveries, postbacks, page changes.
    // X-Hub-Signature-256 is validated before any processing to reject forged requests.
    [HttpPost]
    [EnableRateLimiting(RateLimitPolicies.FacebookWebhook)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> HandleEvent(
        [FromHeader(Name = "X-Hub-Signature-256")] string? signature,
        [FromBody] FacebookWebhookPayload payload)
    {
        string rawBody = (string?)HttpContext.Items["RawBody"] ?? string.Empty;

        if (string.IsNullOrEmpty(signature) || !_facebookWebhookService.IsValidSignature(rawBody, signature))
        {
            return Unauthorized();
        }

        await _facebookWebhookService.ProcessEventAsync(payload, HttpContext.RequestAborted);
        return Ok();
    }
}
