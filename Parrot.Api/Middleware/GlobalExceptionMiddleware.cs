using System.Net;
using System.Text.Json;
using Parrot.Application.Logging;
using Parrot.Domain.Exceptions;

namespace Parrot.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        IRequestContext requestContext = context.RequestServices.GetRequiredService<IRequestContext>();
        string traceId = string.IsNullOrEmpty(requestContext.CorrelationId)
            ? context.TraceIdentifier
            : requestContext.CorrelationId;

        (HttpStatusCode statusCode, string errorCode, string message, IEnumerable<string>? errors) = exception switch
        {
            ValidationException validationEx => (
                validationEx.StatusCode,
                validationEx.ErrorCode,
                validationEx.Message,
                validationEx.Errors
            ),
            DomainException domainEx => (
                domainEx.StatusCode,
                domainEx.ErrorCode,
                domainEx.Message,
                (IEnumerable<string>?)null
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                "INTERNAL_ERROR",
                _environment.IsDevelopment() ? exception.Message : "An unexpected error occurred.",
                (IEnumerable<string>?)null
            )
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(
                exception,
                "Unhandled exception. CorrelationId: {CorrelationId} {HttpMethod} {RequestPath} IP: {IpAddress}",
                traceId,
                requestContext.HttpMethod,
                requestContext.RequestPath,
                requestContext.IpAddress);
        }
        else
        {
            _logger.LogWarning(
                "Domain exception {ErrorCode}: {Message}. CorrelationId: {CorrelationId} {HttpMethod} {RequestPath}",
                errorCode,
                message,
                traceId,
                requestContext.HttpMethod,
                requestContext.RequestPath);
        }

        ErrorResponse response = new ErrorResponse
        {
            ErrorCode = errorCode,
            Message = message,
            Errors = errors,
            TraceId = traceId,
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        JsonSerializerOptions options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
