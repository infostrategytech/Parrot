using Parrot.Application.DTOs.Integrations;

namespace Parrot.Application.Integrations;

public interface IFacebookWebhookService
{
    string GetWebhookUrl();
    bool TryVerifyChallenge(string hubMode, string hubVerifyToken, string hubChallenge, out string echoChallenge);
    bool IsValidSignature(string rawBody, string signatureHeader);
    Task ProcessEventAsync(FacebookWebhookPayload payload, CancellationToken cancellationToken = default);
}
