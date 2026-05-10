using Parrot.Web.Features.Dashboard.Channels.Models;

namespace Parrot.Web.Features.Dashboard.Channels.Services;

public interface IMessageVerificationGrpcClient
{
    Task<MessageCheckResult> CheckForMessageAsync(
        string channelId,
        long sinceUnixSeconds,
        string platform,
        string bearerToken,
        CancellationToken cancellationToken = default);
}
