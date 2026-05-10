namespace Parrot.Web.Features.Dashboard.Channels.Services;

public interface IIntegrationsService
{
    Task<string?> GetWebhookUrlAsync(string platform, string token, CancellationToken cancellationToken = default);
}
