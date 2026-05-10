using MongoDB.Bson.Serialization.Attributes;

namespace Parrot.Domain.Entities.Messages;

[BsonDiscriminator("whatsapp")]
public sealed class WhatsAppMessageDocument : MessageDocument
{
    public required string ChannelId { get; set; }
    public required string ChannelDisplayName { get; set; }
}
