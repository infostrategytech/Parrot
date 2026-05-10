using Parrot.Application.DTOs.Integrations;

namespace Parrot.Application.Integrations;

public interface IPlatformMessageService
{
    Task ProcessAsync(IncomingMessage message, CancellationToken cancellationToken = default);
}
