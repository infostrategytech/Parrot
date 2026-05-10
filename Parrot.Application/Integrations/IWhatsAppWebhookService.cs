using Parrot.Application.DTOs.Integrations;

namespace Parrot.Application.Integrations;

public interface IWhatsAppWebhookService
{
    string GetWebhookUrl();
    bool TryVerifyChallenge(string hubMode, string hubVerifyToken, string hubChallenge, out string echoChallenge);
    Task ProcessEventAsync(WhatsAppWebhookPayload payload);
}
