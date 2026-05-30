namespace Parrot.Web.Features.Dashboard.Channels.Models;

public record MessageCheckResult(bool Received, string? FromNumber, DateTime? ReceivedAt);
