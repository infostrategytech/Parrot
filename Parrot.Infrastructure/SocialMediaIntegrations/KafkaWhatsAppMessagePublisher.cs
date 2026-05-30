using System.Text.Json;
using System.Threading.Channels;
using Confluent.Kafka;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Parrot.Application.DTOs.Integrations;
using Parrot.Application.Integrations;

namespace Parrot.Infrastructure.SocialMediaIntegrations;

public sealed class KafkaMessagePublisher : IMessagePublisher, IHostedService, IDisposable
{
    private readonly Channel<IncomingMessage> _channel;
    private readonly IProducer<string, string> _producer;
    private readonly string _topic;
    private readonly int _batchSize;
    private readonly TimeSpan _flushInterval;
    private readonly ILogger<KafkaMessagePublisher> _logger;
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();
    private Task _drainTask = Task.CompletedTask;

    public KafkaMessagePublisher(IOptions<KafkaSettings> settings, ILogger<KafkaMessagePublisher> logger)
    {
        KafkaSettings s = settings.Value;
        _topic = s.Topic;
        _batchSize = s.ProducerBatchSize;
        _flushInterval = TimeSpan.FromMilliseconds(s.ProducerFlushIntervalMs);
        _logger = logger;

        _channel = Channel.CreateBounded<IncomingMessage>(new BoundedChannelOptions(s.ProducerBatchSize * 20)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.Wait,
        });

        ProducerConfig producerConfig = new ProducerConfig
        {
            BootstrapServers = s.BootstrapServers,
            Acks = Acks.Leader,
        };

        if (s.UseGcpAuth)
        {
            producerConfig.SecurityProtocol = SecurityProtocol.SaslSsl;
            producerConfig.SaslMechanism = SaslMechanism.OAuthBearer;
        }

        ProducerBuilder<string, string> producerBuilder = new ProducerBuilder<string, string>(producerConfig);

        if (s.UseGcpAuth)
        {
            producerBuilder.SetOAuthBearerTokenRefreshHandler((producer, _) =>
            {
                GoogleCredential credential = GoogleCredential
                    .GetApplicationDefault()
                    .CreateScoped("https://www.googleapis.com/auth/cloud-platform");
                string token = credential.UnderlyingCredential
                    .GetAccessTokenForRequestAsync().GetAwaiter().GetResult();
                producer.OAuthBearerSetToken(token, DateTimeOffset.UtcNow.AddMinutes(55).ToUnixTimeMilliseconds(), string.Empty);
            });
        }

        _producer = producerBuilder.Build();
    }

    public async Task PublishAsync(IncomingMessage message, CancellationToken cancellationToken = default)
        => await _channel.Writer.WriteAsync(message, cancellationToken);

    public async Task PublishBatchAsync(IReadOnlyList<IncomingMessage> messages, CancellationToken cancellationToken = default)
    {
        foreach (IncomingMessage msg in messages)
        {
            await _channel.Writer.WriteAsync(msg, cancellationToken);
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _drainTask = Task.Run(() => DrainLoopAsync(_cts.Token), CancellationToken.None);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _channel.Writer.Complete();
        await _cts.CancelAsync();
        await _drainTask.WaitAsync(cancellationToken);
        _producer.Flush(cancellationToken);
    }

    private async Task DrainLoopAsync(CancellationToken ct)
    {
        List<Task> batch = new List<Task>(_batchSize);

        try
        {
            while (!ct.IsCancellationRequested)
            {
                batch.Clear();

                // Wait for the first message or the flush interval, whichever comes first.
                using CancellationTokenSource timer = CancellationTokenSource.CreateLinkedTokenSource(ct);
                timer.CancelAfter(_flushInterval);

                try
                {
                    IncomingMessage first = await _channel.Reader.ReadAsync(timer.Token);
                    batch.Add(ProduceOneAsync(first, ct));
                }
                catch (OperationCanceledException) when (!ct.IsCancellationRequested)
                {
                    // Flush interval elapsed with no messages — loop back and wait again.
                    continue;
                }

                // Greedily drain up to batchSize without blocking.
                while (batch.Count < _batchSize && _channel.Reader.TryRead(out IncomingMessage? msg))
                {
                    batch.Add(ProduceOneAsync(msg, ct));
                }

                await Task.WhenAll(batch);
                _logger.LogDebug("Kafka batch sent. Count={Count}", batch.Count);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (ChannelClosedException)
        {
        }

        // Drain anything left in the channel on shutdown.
        while (_channel.Reader.TryRead(out IncomingMessage? remaining))
        {
            await ProduceOneAsync(remaining, CancellationToken.None);
        }
    }

    private async Task ProduceOneAsync(IncomingMessage message, CancellationToken ct)
    {
        try
        {
            await _producer.ProduceAsync(
                _topic,
                new Message<string, string>
                {
                    Key = message.From,
                    Value = JsonSerializer.Serialize(message),
                },
                ct);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(
                ex,
                "Failed to produce message. MessageId={MessageId}",
                message.MessageId);
        }
    }

    public void Dispose()
    {
        _cts.Dispose();
        _producer.Dispose();
    }
}
