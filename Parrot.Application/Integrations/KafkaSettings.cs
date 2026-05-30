namespace Parrot.Application.Integrations;

public class KafkaSettings
{
    public const string SectionName = "Kafka";

    public string BootstrapServers { get; set; } = "localhost:9092";
    public string Topic { get; set; } = "whatsapp.messages";
    public string ConsumerGroupId { get; set; } = "whatsapp-ingestor";

    /// <summary>Max messages accumulated before a Kafka batch is flushed.</summary>
    public int ProducerBatchSize { get; set; } = 100;

    /// <summary>Maximum milliseconds to wait before flushing a partial batch.</summary>
    public int ProducerFlushIntervalMs { get; set; } = 500;

    /// <summary>Max messages the consumer collects per round before parallel processing.</summary>
    public int ConsumerBatchSize { get; set; } = 50;

    /// <summary>
    /// Use Google Application Default Credentials (SASL_SSL + OAuthBearer).
    /// Required for GCP Managed Service for Apache Kafka.
    /// </summary>
    public bool UseGcpAuth { get; set; } = false;
}
