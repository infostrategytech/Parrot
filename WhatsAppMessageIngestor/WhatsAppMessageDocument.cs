using MongoDB.Bson.Serialization.Attributes;

namespace WhatsAppMessageIngestor;

[BsonDiscriminator("whatsapp")]
public sealed class WhatsAppMessageDocument : MessageDocument
{
    public required string ChannelId { get; set; }
    public required string ChannelDisplayName { get; set; }
}
