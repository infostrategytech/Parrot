using Parrot.Application.DTOs.Integrations;

namespace Parrot.Application.Integrations;

public interface IMessageRepository
{
    Task InsertAsync(IncomingMessage message, CancellationToken cancellationToken = default);

    Task<ReceivedMessageSummary?> FindFirstAsync(
        string channelId,
        DateTime since,
        CancellationToken cancellationToken = default);
}
