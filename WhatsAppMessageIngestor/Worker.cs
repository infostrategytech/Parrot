using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Parrot.Application.DTOs.Integrations;
using Parrot.Application.Integrations;

namespace WhatsAppMessageIngestor;

public sealed class KafkaConsumerWorker : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly IMessageRepository _repository;
    private readonly int _batchSize;
    private readonly string _topic;
    private readonly ILogger<KafkaConsumerWorker> _logger;

    public KafkaConsumerWorker(
        IOptions<KafkaSettings> kafkaSettings,
        IMessageRepository repository,
        ILogger<KafkaConsumerWorker> logger)
    {
        KafkaSettings s = kafkaSettings.Value;
        _topic = s.Topic;
        _batchSize = s.ConsumerBatchSize;
        _repository = repository;
        _logger = logger;

        _consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
        {
            BootstrapServers = s.BootstrapServers,
            GroupId = s.ConsumerGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
        }).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_topic);
        _logger.LogInformation("Kafka consumer subscribed. Topic={Topic} BatchSize={BatchSize}", _topic, _batchSize);

        List<ConsumeResult<string, string>> batch = new List<ConsumeResult<string, string>>(_batchSize);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                batch.Clear();

                // Block until the first message arrives.
                ConsumeResult<string, string> first = _consumer.Consume(stoppingToken);
                batch.Add(first);

                // Greedily collect more without blocking until the batch is full.
                while (batch.Count < _batchSize)
                {
                    ConsumeResult<string, string>? next = _consumer.Consume(TimeSpan.FromMilliseconds(10));
                    if (next is null)
                    {
                        break;
                    }

                    batch.Add(next);
                }

                // Process the batch in parallel — MongoDB writes are independent.
                await Parallel.ForEachAsync(
                    batch,
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = Environment.ProcessorCount,
                        CancellationToken = stoppingToken,
                    },
                    async (result, ct) =>
                    {
                        try
                        {
                            IncomingMessage? message = JsonSerializer.Deserialize<IncomingMessage>(result.Message.Value);

                            if (message is null)
                            {
                                _logger.LogWarning(
                                    "Null deserialization. Partition={Partition} Offset={Offset}",
                                    result.Partition.Value,
                                    result.Offset.Value);
                                return;
                            }

                            await _repository.InsertAsync(message, ct);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(
                                ex,
                                "Failed to process message. Partition={Partition} Offset={Offset}",
                                result.Partition.Value,
                                result.Offset.Value);
                        }
                    });

                // Commit after the full batch is written — per-partition highest offset.
                IEnumerable<TopicPartitionOffset> offsets = batch
                    .GroupBy(r => r.TopicPartition)
                    .Select(g => new TopicPartitionOffset(g.Key, g.Max(r => r.Offset) + 1));

                _consumer.Commit(offsets);

                _logger.LogDebug("Batch committed. Count={Count}", batch.Count);
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown.
        }
        catch (ConsumeException ex)
        {
            _logger.LogError(ex, "Kafka consume error. Reason={Reason}", ex.Error.Reason);
        }
        finally
        {
            _consumer.Close();
        }
    }

    public override void Dispose()
    {
        _consumer.Dispose();
        base.Dispose();
    }
}
