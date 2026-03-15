namespace Parrot.Api.Middleware;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }

    public static IApplicationBuilder UseRequestContext(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestContextMiddleware>();
    }
}
