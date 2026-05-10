using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Parrot.Application.DTOs.Integrations;
using Parrot.Application.Integrations;
using Parrot.Grpc.Contracts.Messaging.V1;

namespace Parrot.APi.Grpc.Services;

[Authorize]
public sealed class MessageVerificationGrpcService(
    IMessageRepository repository,
    ILogger<MessageVerificationGrpcService> logger)
    : MessageVerificationService.MessageVerificationServiceBase
{
    public override async Task<CheckForMessageResponse> CheckForMessage(
        CheckForMessageRequest request,
        ServerCallContext context)
    {
        DateTime since = DateTimeOffset.FromUnixTimeSeconds(request.SinceUnixSeconds).UtcDateTime;

        logger.LogInformation(
            "Checking for message on channel {ChannelId} (platform={Platform}) since {Since}",
            request.ChannelId,
            request.Platform,
            since);

        ReceivedMessageSummary? summary = await repository.FindFirstAsync(
            request.ChannelId,
            since,
            context.CancellationToken);

        if (summary is null)
            return new CheckForMessageResponse { MessageReceived = false };

        return new CheckForMessageResponse
        {
            MessageReceived = true,
            MessageId = summary.MessageId,
            FromNumber = summary.From,
            ReceivedAtUnixSeconds = new DateTimeOffset(summary.ReceivedAt, TimeSpan.Zero).ToUnixTimeSeconds(),
        };
    }
}
