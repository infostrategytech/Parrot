using Parrot.Application.DTOs.Integrations;
using Parrot.Domain.Enums;

namespace Parrot.Application.Integrations;

public sealed class WhatsAppMessageHandler : IPlatformMessageHandler
{
    private readonly IMessageRepository _repository;

    public MessagingPlatform Platform => MessagingPlatform.WhatsApp;

    public WhatsAppMessageHandler(IMessageRepository repository)
    {
        _repository = repository;
    }

    public Task HandleAsync(IncomingMessage message, CancellationToken cancellationToken = default)
    {
        IncomingMessage remapped = Remap(message);
        return _repository.InsertAsync(remapped, cancellationToken);
    }

    private static IncomingMessage Remap(IncomingMessage message) => message with
    {
        TextBody = message.Type == "text" ? message.TextBody : null,
    };
}
