using Parrot.Application.DTOs.Integrations;

namespace WhatsAppMessageIngestor;

public interface IMessageRepository
{
    Task InsertAsync(IncomingMessage message, CancellationToken cancellationToken = default);
}
