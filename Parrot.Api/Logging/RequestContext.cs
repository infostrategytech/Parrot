namespace Parrot.Api.Logging;

public class RequestContext : IRequestContext
{
    public string CorrelationId { get; set; } = string.Empty;

    public string? IpAddress { get; set; }

    public string? HttpMethod { get; set; }

    public string? RequestPath { get; set; }

    public string? UserAgent { get; set; }
}
