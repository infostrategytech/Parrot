namespace Parrot.Api.Middleware;

public record ErrorResponse
{
    public bool Success => false;
    public required string ErrorCode { get; init; }
    public required string Message { get; init; }
    public IEnumerable<string>? Errors { get; init; }
    public string? TraceId { get; init; }
}
