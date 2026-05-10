using Microsoft.Extensions.Options;
using Parrot.Application.DTOs.Integrations;
using Parrot.Application.Logging;

namespace Parrot.Application.Integrations;

public class WhatsAppWebhookService : IWhatsAppWebhookService
{
    private readonly WhatsAppSettings _settings;
    private readonly IAppLogger<WhatsAppWebhookService> _logger;

    public WhatsAppWebhookService(IOptions<WhatsAppSettings> settings, IAppLogger<WhatsAppWebhookService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public string GetWebhookUrl() =>
        $"{_settings.WebhookBaseUrl.TrimEnd('/')}/api/webhooks/whatsapp";

    public bool TryVerifyChallenge(string hubMode, string hubVerifyToken, string hubChallenge, out string echoChallenge)
    {
        echoChallenge = string.Empty;

        if (hubMode != "subscribe" || hubVerifyToken != _settings.VerifyToken)
        {
            _logger.LogWarning("WhatsApp webhook challenge rejected. Mode={Mode} TokenMatch={Match}", hubMode, hubVerifyToken == _settings.VerifyToken);
            return false;
        }

        echoChallenge = hubChallenge;
        return true;
    }

    public Task ProcessEventAsync(WhatsAppWebhookPayload payload)
    {
        foreach (WhatsAppEntry entry in payload.Entry)
        {
            foreach (WhatsAppChange change in entry.Changes)
            {
                if (change.Field != "messages")
                {
                    continue;
                }

                if (change.Value.Messages is { Count: > 0 } messages)
                {
                    foreach (WhatsAppMessage msg in messages)
                    {
                        _logger.LogInformation("WhatsApp message received. From={From} Type={Type} MessageId={Id}", msg.From, msg.Type, msg.Id);
                    }
                }

                if (change.Value.Statuses is { Count: > 0 } statuses)
                {
                    foreach (WhatsAppMessageStatus status in statuses)
                    {
                        _logger.LogInformation("WhatsApp message status update. MessageId={Id} Status={Status}", status.Id, status.Status);
                    }
                }
            }
        }

        return Task.CompletedTask;
    }
}
