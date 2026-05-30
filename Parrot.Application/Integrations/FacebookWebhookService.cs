using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Parrot.Application.DTOs.Integrations;
using Parrot.Application.Logging;
using Parrot.Domain.Enums;

namespace Parrot.Application.Integrations;

public sealed class FacebookWebhookService : IFacebookWebhookService
{
    private readonly FacebookSettings _settings;
    private readonly IFacebookPageRegistry _pageRegistry;
    private readonly IMessagePublisher _publisher;
    private readonly IAppLogger<FacebookWebhookService> _logger;

    public FacebookWebhookService(
        IOptions<FacebookSettings> settings,
        IFacebookPageRegistry pageRegistry,
        IMessagePublisher publisher,
        IAppLogger<FacebookWebhookService> logger)
    {
        _settings = settings.Value;
        _pageRegistry = pageRegistry;
        _publisher = publisher;
        _logger = logger;
    }

    public string GetWebhookUrl() =>
        $"{_settings.WebhookBaseUrl.TrimEnd('/')}/api/webhooks/facebook";

    public bool TryVerifyChallenge(string hubMode, string hubVerifyToken, string hubChallenge, out string echoChallenge)
    {
        echoChallenge = string.Empty;

        if (hubMode != "subscribe" || hubVerifyToken != _settings.VerifyToken)
        {
            _logger.LogWarning("Facebook webhook challenge rejected. Mode={Mode} TokenMatch={Match}", hubMode, hubVerifyToken == _settings.VerifyToken);
            return false;
        }

        echoChallenge = hubChallenge;
        return true;
    }

    public bool IsValidSignature(string rawBody, string signatureHeader)
    {
        if (!signatureHeader.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase))
            return false;

        string receivedHash = signatureHeader["sha256=".Length..];

        byte[] key = Encoding.UTF8.GetBytes(_settings.AppSecret);
        byte[] body = Encoding.UTF8.GetBytes(rawBody);
        byte[] expectedBytes = HMACSHA256.HashData(key, body);
        string expectedHash = Convert.ToHexString(expectedBytes).ToLowerInvariant();

        return CryptographicOperations.FixedTimeEquals(
            Encoding.ASCII.GetBytes(receivedHash.ToLowerInvariant()),
            Encoding.ASCII.GetBytes(expectedHash));
    }

    public async Task ProcessEventAsync(FacebookWebhookPayload payload, CancellationToken cancellationToken = default)
    {
        if (payload.Object is not ("page" or "instagram"))
        {
            _logger.LogWarning("Unrecognised Facebook webhook object type: {Object}", payload.Object);
            return;
        }

        foreach (FacebookEntry entry in payload.Entry)
        {
            string? tenantId = await _pageRegistry.ResolveTenantAsync(entry.Id, cancellationToken);

            if (tenantId is null)
            {
                _logger.LogWarning("Facebook event received for unregistered page. PageId={PageId}", entry.Id);
                continue;
            }

            if (entry.Messaging is { Count: > 0 } messagingEvents)
                await ProcessMessagingEventsAsync(entry.Id, tenantId, messagingEvents, cancellationToken);

            if (entry.Changes is { Count: > 0 } changes)
                ProcessPageChanges(entry.Id, tenantId, changes);
        }
    }

    private async Task ProcessMessagingEventsAsync(
        string pageId,
        string tenantId,
        List<FacebookMessagingEvent> events,
        CancellationToken cancellationToken)
    {
        List<IncomingMessage> batch = new(events.Count);

        foreach (FacebookMessagingEvent evt in events)
        {
            // Ignore echo messages sent by the page itself
            if (evt.Message?.IsEcho == true)
                continue;

            if (evt.Message is not null)
            {
                _logger.LogInformation(
                    "Facebook message received. TenantId={TenantId} PageId={PageId} SenderId={Sender} Mid={Mid}",
                    tenantId, pageId, evt.Sender.Id, evt.Message.Mid);

                string type = evt.Message.Attachments is { Count: > 0 }
                    ? evt.Message.Attachments[0].Type
                    : "text";

                batch.Add(new IncomingMessage
                {
                    MessageId = evt.Message.Mid,
                    Platform = MessagingPlatform.Facebook,
                    ExternalAccountId = tenantId,
                    From = evt.Sender.Id,
                    Timestamp = evt.Timestamp.ToString(),
                    Type = type,
                    TextBody = evt.Message.Text,
                    ChannelId = pageId,
                    ChannelDisplayName = pageId,
                    ReceivedAt = DateTime.UtcNow,
                });

                continue;
            }

            if (evt.Postback is not null)
            {
                _logger.LogInformation(
                    "Facebook postback received. TenantId={TenantId} PageId={PageId} SenderId={Sender} Payload={Payload}",
                    tenantId, pageId, evt.Sender.Id, evt.Postback.Payload);

                batch.Add(new IncomingMessage
                {
                    MessageId = evt.Postback.Mid,
                    Platform = MessagingPlatform.Facebook,
                    ExternalAccountId = tenantId,
                    From = evt.Sender.Id,
                    Timestamp = evt.Timestamp.ToString(),
                    Type = "postback",
                    TextBody = evt.Postback.Payload,
                    ChannelId = pageId,
                    ChannelDisplayName = pageId,
                    ReceivedAt = DateTime.UtcNow,
                });

                continue;
            }

            if (evt.Delivery is not null)
            {
                _logger.LogInformation(
                    "Facebook delivery confirmation. TenantId={TenantId} PageId={PageId} Watermark={Watermark}",
                    tenantId, pageId, evt.Delivery.Watermark);
                continue;
            }

            if (evt.Read is not null)
            {
                _logger.LogInformation(
                    "Facebook read confirmation. TenantId={TenantId} PageId={PageId} Watermark={Watermark}",
                    tenantId, pageId, evt.Read.Watermark);
                continue;
            }

            if (evt.Reaction is not null)
            {
                _logger.LogInformation(
                    "Facebook reaction. TenantId={TenantId} PageId={PageId} Mid={Mid} Action={Action}",
                    tenantId, pageId, evt.Reaction.Mid, evt.Reaction.Action);
            }
        }

        if (batch.Count > 0)
            await _publisher.PublishBatchAsync(batch, cancellationToken);
    }

    private void ProcessPageChanges(string pageId, string tenantId, List<FacebookChange> changes)
    {
        foreach (FacebookChange change in changes)
        {
            _logger.LogInformation(
                "Facebook page change. TenantId={TenantId} PageId={PageId} Field={Field}",
                tenantId, pageId, change.Field);
        }
    }
}
