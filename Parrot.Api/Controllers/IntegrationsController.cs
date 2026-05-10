using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parrot.Api.Middleware;
using Parrot.Application.Integrations;

namespace Parrot.Api.Rest.Controllers;

[Route("api/integrations")]
[ApiController]
[Authorize]
public class IntegrationsController : ControllerBase
{
    private readonly IWhatsAppWebhookService _whatsAppWebhookService;

    public IntegrationsController(IWhatsAppWebhookService whatsAppWebhookService)
    {
        _whatsAppWebhookService = whatsAppWebhookService;
    }

    [HttpGet("business-webhook/{messagingPlatform}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult GetWebhookUrl(string messagingPlatform)
    {
        string webhookUrl = messagingPlatform.ToLowerInvariant() switch
        {
            "whatsapp" => _whatsAppWebhookService.GetWebhookUrl(),
            _ => string.Empty,
        };

        if (string.IsNullOrEmpty(webhookUrl))
        {
            return NotFound(new { Message = $"No webhook URL configured for platform '{messagingPlatform}'." });
        }

        return Ok(new { WebhookUrl = webhookUrl });
    }
}
