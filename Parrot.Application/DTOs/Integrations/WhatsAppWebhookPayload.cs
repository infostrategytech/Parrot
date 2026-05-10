using System.Text.Json.Serialization;

namespace Parrot.Application.DTOs.Integrations;

public record WhatsAppWebhookPayload
{
    [JsonPropertyName("object")]
    public required string Object { get; init; }

    [JsonPropertyName("entry")]
    public required List<WhatsAppEntry> Entry { get; init; }
}

public record WhatsAppEntry
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("changes")]
    public required List<WhatsAppChange> Changes { get; init; }
}

public record WhatsAppChange
{
    [JsonPropertyName("value")]
    public required WhatsAppChangeValue Value { get; init; }

    [JsonPropertyName("field")]
    public required string Field { get; init; }
}

public record WhatsAppChangeValue
{
    [JsonPropertyName("messaging_product")]
    public required string MessagingProduct { get; init; }

    [JsonPropertyName("metadata")]
    public required WhatsAppMetadata Metadata { get; init; }

    [JsonPropertyName("contacts")]
    public List<WhatsAppContact>? Contacts { get; init; }

    [JsonPropertyName("messages")]
    public List<WhatsAppMessage>? Messages { get; init; }

    [JsonPropertyName("statuses")]
    public List<WhatsAppMessageStatus>? Statuses { get; init; }
}

public record WhatsAppMetadata
{
    [JsonPropertyName("display_phone_number")]
    public required string DisplayPhoneNumber { get; init; }

    [JsonPropertyName("phone_number_id")]
    public required string PhoneNumberId { get; init; }
}

public record WhatsAppContact
{
    [JsonPropertyName("profile")]
    public required WhatsAppProfile Profile { get; init; }

    [JsonPropertyName("wa_id")]
    public required string WaId { get; init; }
}

public record WhatsAppProfile
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }
}

public record WhatsAppMessage
{
    [JsonPropertyName("from")]
    public required string From { get; init; }

    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("timestamp")]
    public required string Timestamp { get; init; }

    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("text")]
    public WhatsAppTextBody? Text { get; init; }
}

public record WhatsAppTextBody
{
    [JsonPropertyName("body")]
    public required string Body { get; init; }
}

public record WhatsAppMessageStatus
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("timestamp")]
    public required string Timestamp { get; init; }

    [JsonPropertyName("recipient_id")]
    public required string RecipientId { get; init; }
}
