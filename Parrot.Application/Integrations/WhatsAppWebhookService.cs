using Microsoft.Extensions.Options;
using Parrot.Application.DTOs.Integrations;
using Parrot.Application.Logging;

namespace Parrot.Application.Integrations;

public class WhatsAppWebhookService : IWhatsAppWebhookService
{
    private readonly WhatsAppSettings _settings;
    private readonly IAppLogger<WhatsAppWebhookService> _logger;
    private readonly IMessagePublisher _publisher;

    public WhatsAppWebhookService(
        IOptions<WhatsAppSettings> settings,
        IAppLogger<WhatsAppWebhookService> logger,
        IMessagePublisher publisher)
    {
        _settings = settings.Value;
        _logger = logger;
        _publisher = publisher;
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

    public async Task ProcessEventAsync(WhatsAppWebhookPayload payload)
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
                    Dictionary<string, string> contactNames = change.Value.Contacts?
                        .ToDictionary(c => c.WaId, c => c.Profile.Name)
                        ?? [];

                    List<IncomingMessage> batch = new List<IncomingMessage>(messages.Count);

                    foreach (WhatsAppMessage msg in messages)
                    {
                        _logger.LogInformation(
                            "WhatsApp message received. From={From} Type={Type} MessageId={Id}",
                            msg.From,
                            msg.Type,
                            msg.Id);

                        batch.Add(new IncomingMessage
                        {
                            MessageId = msg.Id,
                            Platform = MessagingPlatform.WhatsApp,
                            ExternalAccountId = entry.Id,
                            From = msg.From,
                            Timestamp = msg.Timestamp,
                            Type = msg.Type,
                            TextBody = msg.Text?.Body,
                            ChannelId = change.Value.Metadata.PhoneNumberId,
                            ChannelDisplayName = change.Value.Metadata.DisplayPhoneNumber,
                            ContactName = contactNames.GetValueOrDefault(msg.From),
                            ReceivedAt = DateTime.UtcNow,
                        });
                    }

                    await _publisher.PublishBatchAsync(batch);
                }

                if (change.Value.Statuses is { Count: > 0 } statuses)
                {
                    foreach (WhatsAppMessageStatus status in statuses)
                    {
                        _logger.LogInformation(
                            "WhatsApp status update. MessageId={Id} Status={Status}",
                            status.Id,
                            status.Status);
                    }
                }
            }
        }
    }
}
