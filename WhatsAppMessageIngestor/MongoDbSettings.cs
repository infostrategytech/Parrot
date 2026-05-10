namespace WhatsAppMessageIngestor;

public sealed class MongoDbSettings
{
    public const string SectionName = "MongoDB";

    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string Database { get; set; } = "parrot";
    public string Collection { get; set; } = "whatsapp_messages";
}
