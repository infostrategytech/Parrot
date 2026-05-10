using Parrot.Application.DTOs.Integrations;

namespace Parrot.Application.Integrations;

public interface IMessagePublisher
{
    Task PublishAsync(IncomingMessage message, CancellationToken cancellationToken = default);
    Task PublishBatchAsync(IReadOnlyList<IncomingMessage> messages, CancellationToken cancellationToken = default);
}
