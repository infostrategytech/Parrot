using Parrot.Application.DTOs.Integrations;

namespace Parrot.Application.Integrations;

public sealed class PlatformMessageService : IPlatformMessageService
{
    private readonly IPlatformMessageHandlerFactory _factory;

    public PlatformMessageService(IPlatformMessageHandlerFactory factory)
    {
        _factory = factory;
    }

    public Task ProcessAsync(IncomingMessage message, CancellationToken cancellationToken = default)
    {
        IPlatformMessageHandler handler = _factory.GetHandler(message.Platform);
        return handler.HandleAsync(message, cancellationToken);
    }
}
