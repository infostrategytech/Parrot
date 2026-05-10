using Parrot.Application.DTOs.Integrations;
using Parrot.Domain.Enums;

namespace Parrot.Application.Integrations;

public interface IPlatformMessageHandler
{
    MessagingPlatform Platform { get; }
    Task HandleAsync(IncomingMessage message, CancellationToken cancellationToken = default);
}
