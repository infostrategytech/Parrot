using Parrot.Domain.Enums;

namespace Parrot.Application.Integrations;

public interface IPlatformMessageHandlerFactory
{
    IPlatformMessageHandler GetHandler(MessagingPlatform platform);
}
