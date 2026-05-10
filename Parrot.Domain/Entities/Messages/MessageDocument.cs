using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Parrot.Domain.Enums;

namespace Parrot.Domain.Entities.Messages;

[BsonDiscriminator(Required = true)]
[BsonKnownTypes(typeof(WhatsAppMessageDocument))]
public abstract class MessageDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public required string MessageId { get; set; }
    public required MessagingPlatform Platform { get; set; }
    public required string ExternalAccountId { get; set; }
    public required string From { get; set; }
    public required string Timestamp { get; set; }
    public required string Type { get; set; }
    public string? TextBody { get; set; }
    public string? ContactName { get; set; }
    public DateTime ReceivedAt { get; set; }
    public DateTime StoredAt { get; set; }
}
