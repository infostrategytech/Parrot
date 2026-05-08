namespace Parrot.Application.Logging;

public interface IRequestContext
{
    string CorrelationId { get; }

    string? IpAddress { get; }

    string? HttpMethod { get; }

    string? RequestPath { get; }

    string? UserAgent { get; }
}
