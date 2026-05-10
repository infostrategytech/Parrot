using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Parrot.Application.DTOs.Integrations;

namespace WhatsAppMessageIngestor;

public sealed class MongoMessageRepository : IMessageRepository
{
    private readonly IMongoCollection<MessageDocument> _collection;

    public MongoMessageRepository(IMongoClient mongoClient, IOptions<MongoDbSettings> settings)
    {
        IMongoDatabase db = mongoClient.GetDatabase(settings.Value.Database);
        _collection = db.GetCollection<MessageDocument>(settings.Value.Collection);

        IndexKeysDefinition<MessageDocument> indexKeys =
            Builders<MessageDocument>.IndexKeys.Ascending(x => x.MessageId);
        _collection.Indexes.CreateOne(new CreateIndexModel<MessageDocument>(
            indexKeys, new CreateIndexOptions { Unique = true }));
    }

    public async Task InsertAsync(IncomingMessage message, CancellationToken cancellationToken = default)
    {
        MessageDocument doc = ToDocument(message);

        try
        {
            await _collection.InsertOneAsync(doc, cancellationToken: cancellationToken);
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            // Already stored — safe to skip on Kafka redelivery.
        }
    }

    private static MessageDocument ToDocument(IncomingMessage message) => message.Platform switch
    {
        MessagingPlatform.WhatsApp => new WhatsAppMessageDocument
        {
            MessageId = message.MessageId,
            Platform = message.Platform,
            ExternalAccountId = message.ExternalAccountId,
            From = message.From,
            Timestamp = message.Timestamp,
            Type = message.Type,
            TextBody = message.TextBody,
            ContactName = message.ContactName,
            ReceivedAt = message.ReceivedAt,
            StoredAt = DateTime.UtcNow,
            ChannelId = message.ChannelId,
            ChannelDisplayName = message.ChannelDisplayName,
        },
        _ => throw new ArgumentOutOfRangeException(nameof(message.Platform), message.Platform, "Unsupported messaging platform."),
    };
}
