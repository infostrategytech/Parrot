using Parrot.Domain.Enums;

namespace Parrot.Application.Integrations;

public sealed class PlatformMessageHandlerFactory : IPlatformMessageHandlerFactory
{
    private readonly IReadOnlyDictionary<MessagingPlatform, IPlatformMessageHandler> _handlers;

    public PlatformMessageHandlerFactory(IEnumerable<IPlatformMessageHandler> handlers)
    {
        _handlers = handlers.ToDictionary(h => h.Platform);
    }

    public IPlatformMessageHandler GetHandler(MessagingPlatform platform)
    {
        if (!_handlers.TryGetValue(platform, out IPlatformMessageHandler? handler))
            throw new InvalidOperationException($"No handler registered for platform '{platform}'.");

        return handler;
    }
}
