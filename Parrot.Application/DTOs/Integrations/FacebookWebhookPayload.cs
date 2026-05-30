using System.Text.Json.Serialization;

namespace Parrot.Application.DTOs.Integrations;

public record FacebookWebhookPayload
{
    [JsonPropertyName("object")]
    public required string Object { get; init; }

    [JsonPropertyName("entry")]
    public required List<FacebookEntry> Entry { get; init; }
}

public record FacebookEntry
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("time")]
    public long Time { get; init; }

    // Messenger messages/events
    [JsonPropertyName("messaging")]
    public List<FacebookMessagingEvent>? Messaging { get; init; }

    // Page-level changes (feed, mentions, etc.)
    [JsonPropertyName("changes")]
    public List<FacebookChange>? Changes { get; init; }
}

public record FacebookMessagingEvent
{
    [JsonPropertyName("sender")]
    public required FacebookParticipant Sender { get; init; }

    [JsonPropertyName("recipient")]
    public required FacebookParticipant Recipient { get; init; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; init; }

    [JsonPropertyName("message")]
    public FacebookMessage? Message { get; init; }

    [JsonPropertyName("delivery")]
    public FacebookDelivery? Delivery { get; init; }

    [JsonPropertyName("read")]
    public FacebookRead? Read { get; init; }

    [JsonPropertyName("reaction")]
    public FacebookReaction? Reaction { get; init; }

    [JsonPropertyName("postback")]
    public FacebookPostback? Postback { get; init; }
}

public record FacebookParticipant
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }
}

public record FacebookMessage
{
    [JsonPropertyName("mid")]
    public required string Mid { get; init; }

    [JsonPropertyName("text")]
    public string? Text { get; init; }

    [JsonPropertyName("attachments")]
    public List<FacebookAttachment>? Attachments { get; init; }

    [JsonPropertyName("is_echo")]
    public bool IsEcho { get; init; }

    [JsonPropertyName("reply_to")]
    public FacebookReplyTo? ReplyTo { get; init; }
}

public record FacebookAttachment
{
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("payload")]
    public FacebookAttachmentPayload? Payload { get; init; }
}

public record FacebookAttachmentPayload
{
    [JsonPropertyName("url")]
    public string? Url { get; init; }
}

public record FacebookReplyTo
{
    [JsonPropertyName("mid")]
    public required string Mid { get; init; }
}

public record FacebookDelivery
{
    [JsonPropertyName("mids")]
    public List<string>? Mids { get; init; }

    [JsonPropertyName("watermark")]
    public long Watermark { get; init; }
}

public record FacebookRead
{
    [JsonPropertyName("watermark")]
    public long Watermark { get; init; }
}

public record FacebookReaction
{
    [JsonPropertyName("mid")]
    public required string Mid { get; init; }

    [JsonPropertyName("action")]
    public required string Action { get; init; }

    [JsonPropertyName("reaction")]
    public string? Emoji { get; init; }
}

public record FacebookPostback
{
    [JsonPropertyName("mid")]
    public required string Mid { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("payload")]
    public required string Payload { get; init; }
}

public record FacebookChange
{
    [JsonPropertyName("field")]
    public required string Field { get; init; }

    [JsonPropertyName("value")]
    public required System.Text.Json.JsonElement Value { get; init; }
}
