using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Parrot.Application.Integrations;
using Parrot.Infrastructure.Messaging;
using WhatsAppMessageIngestor;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<KafkaSettings>(
    builder.Configuration.GetSection(KafkaSettings.SectionName));

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection(MongoDbSettings.SectionName));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    MongoDbSettings settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddSingleton<IMessageRepository, MongoMessageRepository>();

builder.Services.AddHostedService<KafkaConsumerWorker>();

IHost host = builder.Build();
host.Run();
