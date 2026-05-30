namespace Parrot.Api.Middleware;

// Captures the raw request body for routes that need to validate HMAC signatures
// (e.g. Facebook X-Hub-Signature-256). Must run before the request body is consumed
// by model binding. Stored in HttpContext.Items["RawBody"].
public sealed class RawBodyMiddleware
{
    private readonly RequestDelegate _next;

    public RawBodyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/api/webhooks", StringComparison.OrdinalIgnoreCase))
        {
            context.Request.EnableBuffering();

            using StreamReader reader = new(context.Request.Body, leaveOpen: true);
            context.Items["RawBody"] = await reader.ReadToEndAsync(context.RequestAborted);

            context.Request.Body.Position = 0;
        }

        await _next(context);
    }
}
