namespace Parrot.Application.DTOs.Integrations;

public sealed record ReceivedMessageSummary(string MessageId, string From, DateTime ReceivedAt);
