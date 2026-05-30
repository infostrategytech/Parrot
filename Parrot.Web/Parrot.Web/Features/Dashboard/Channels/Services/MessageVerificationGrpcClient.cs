using Grpc.Core;
using Parrot.Grpc.Contracts.Messaging.V1;
using Parrot.Web.Features.Dashboard.Channels.Models;

namespace Parrot.Web.Features.Dashboard.Channels.Services;

public class MessageVerificationGrpcClient : IMessageVerificationGrpcClient
{
    private readonly MessageVerificationService.MessageVerificationServiceClient _client;

    public MessageVerificationGrpcClient(MessageVerificationService.MessageVerificationServiceClient client)
    {
        _client = client;
    }

    public async Task<MessageCheckResult> CheckForMessageAsync(
        string channelId,
        long sinceUnixSeconds,
        string platform,
        string bearerToken,
        CancellationToken cancellationToken = default)
    {
        var headers = new Metadata { { "Authorization", $"Bearer {bearerToken}" } };
        CheckForMessageResponse response = await _client.CheckForMessageAsync(
            new CheckForMessageRequest
            {
                ChannelId = channelId,
                SinceUnixSeconds = sinceUnixSeconds,
                Platform = platform,
            },
            new CallOptions(headers: headers, cancellationToken: cancellationToken));

        if (!response.MessageReceived)
            return new MessageCheckResult(false, null, null);

        DateTime receivedAt = DateTimeOffset.FromUnixTimeSeconds(response.ReceivedAtUnixSeconds).UtcDateTime;
        return new MessageCheckResult(true, response.FromNumber, receivedAt);
    }
}
