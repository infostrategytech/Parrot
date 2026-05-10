namespace Parrot.Application.DTOs.Integrations;

public enum MessagingPlatform
{
    WhatsApp = 1,
}

public record IncomingMessage
{
    public required string MessageId { get; init; }
    public required MessagingPlatform Platform { get; init; }
    public required string ExternalAccountId { get; init; }
    public required string From { get; init; }
    public required string Timestamp { get; init; }
    public required string Type { get; init; }
    public string? TextBody { get; init; }
    public required string ChannelId { get; init; }
    public required string ChannelDisplayName { get; init; }
    public string? ContactName { get; init; }
    public DateTime ReceivedAt { get; init; }
}
