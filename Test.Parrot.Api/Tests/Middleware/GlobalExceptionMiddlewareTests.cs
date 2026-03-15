using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Parrot.Api.Logging;
using Parrot.Api.Middleware;
using Parrot.Domain.Exceptions;

namespace Test.Parrot.Api.Tests.Middleware;

public class GlobalExceptionMiddlewareTests
{
    private static GlobalExceptionMiddleware CreateMiddleware(RequestDelegate next, bool isDevelopment = true)
    {
        Mock<IHostEnvironment> env = new Mock<IHostEnvironment>();
        env.Setup(e => e.EnvironmentName)
           .Returns(isDevelopment ? Environments.Development : Environments.Production);

        return new GlobalExceptionMiddleware(
            next,
            NullLogger<GlobalExceptionMiddleware>.Instance,
            env.Object);
    }

    private static DefaultHttpContext CreateContext(string correlationId = "trace-123")
    {
        RequestContext requestContext = new RequestContext
        {
            CorrelationId = correlationId,
            HttpMethod = "GET",
            RequestPath = "/test",
            IpAddress = "127.0.0.1",
        };

        ServiceCollection services = new ServiceCollection();
        services.AddSingleton<IRequestContext>(requestContext);

        DefaultHttpContext context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.RequestServices = services.BuildServiceProvider();
        return context;
    }

    private static async Task<(int StatusCode, JsonDocument Doc)> InvokeAsync(
        RequestDelegate next,
        bool isDevelopment = true,
        string correlationId = "trace-abc")
    {
        GlobalExceptionMiddleware middleware = CreateMiddleware(next, isDevelopment);
        DefaultHttpContext context = CreateContext(correlationId);

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        string body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        return (context.Response.StatusCode, JsonDocument.Parse(body));
    }

    [Fact]
    public async Task InvokeAsync_ValidationException_Returns400()
    {
        RequestDelegate next = _ => throw new ValidationException("Bad field");
        (int status, JsonDocument doc) = await InvokeAsync(next);

        Assert.Equal(400, status);
        Assert.Equal("VALIDATION_ERROR", doc.RootElement.GetProperty("errorCode").GetString());
        Assert.Equal("Bad field", doc.RootElement.GetProperty("message").GetString());
    }

    [Fact]
    public async Task InvokeAsync_ValidationException_WithMultipleErrors_ReturnsErrorsArray()
    {
        RequestDelegate next = _ => throw new ValidationException(["Field1 required", "Field2 invalid"]);
        (int status, JsonDocument doc) = await InvokeAsync(next);

        Assert.Equal(400, status);
        JsonElement errors = doc.RootElement.GetProperty("errors");
        Assert.Equal(2, errors.GetArrayLength());
    }

    [Fact]
    public async Task InvokeAsync_ConflictException_Returns409()
    {
        RequestDelegate next = _ => throw new ConflictException("Already exists");
        (int status, JsonDocument doc) = await InvokeAsync(next);

        Assert.Equal(409, status);
        Assert.Equal("CONFLICT", doc.RootElement.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task InvokeAsync_UnauthorizedException_Returns401()
    {
        RequestDelegate next = _ => throw new UnauthorizedException("Not authorised");
        (int status, JsonDocument doc) = await InvokeAsync(next);

        Assert.Equal(401, status);
        Assert.Equal("UNAUTHORIZED", doc.RootElement.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task InvokeAsync_NotFoundException_Returns404()
    {
        RequestDelegate next = _ => throw new NotFoundException("Entity not found");
        (int status, JsonDocument doc) = await InvokeAsync(next);

        Assert.Equal(404, status);
        Assert.Equal("NOT_FOUND", doc.RootElement.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task InvokeAsync_ForbiddenException_Returns403()
    {
        RequestDelegate next = _ => throw new ForbiddenException("Access denied");
        (int status, JsonDocument doc) = await InvokeAsync(next);

        Assert.Equal(403, status);
        Assert.Equal("FORBIDDEN", doc.RootElement.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task InvokeAsync_UnhandledException_InDevelopment_ExposesActualMessage()
    {
        RequestDelegate next = _ => throw new InvalidOperationException("Sensitive internal details");
        (int status, JsonDocument doc) = await InvokeAsync(next, isDevelopment: true);

        Assert.Equal((int)HttpStatusCode.InternalServerError, status);
        Assert.Equal("INTERNAL_ERROR", doc.RootElement.GetProperty("errorCode").GetString());
        Assert.Equal("Sensitive internal details", doc.RootElement.GetProperty("message").GetString());
    }

    [Fact]
    public async Task InvokeAsync_UnhandledException_InProduction_ReturnsGenericMessage()
    {
        RequestDelegate next = _ => throw new InvalidOperationException("Sensitive internal details");
        (int status, JsonDocument doc) = await InvokeAsync(next, isDevelopment: false);

        Assert.Equal((int)HttpStatusCode.InternalServerError, status);
        Assert.Equal("INTERNAL_ERROR", doc.RootElement.GetProperty("errorCode").GetString());
        Assert.NotEqual(
            "Sensitive internal details",
            doc.RootElement.GetProperty("message").GetString());
    }

    [Fact]
    public async Task InvokeAsync_ResponseContainsCorrelationIdAsTraceId()
    {
        RequestDelegate next = _ => throw new NotFoundException("Gone");
        (_, JsonDocument doc) = await InvokeAsync(next, correlationId: "my-trace-999");

        Assert.Equal("my-trace-999", doc.RootElement.GetProperty("traceId").GetString());
    }

    [Fact]
    public async Task InvokeAsync_ResponseSuccessFieldIsFalse()
    {
        RequestDelegate next = _ => throw new ConflictException("Conflict");
        (_, JsonDocument doc) = await InvokeAsync(next);

        Assert.False(doc.RootElement.GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task InvokeAsync_WhenNoException_PassesThroughWithoutInterference()
    {
        bool nextCalled = false;
        RequestDelegate next = ctx =>
        {
            nextCalled = true;
            ctx.Response.StatusCode = 200;
            return Task.CompletedTask;
        };

        GlobalExceptionMiddleware middleware = CreateMiddleware(next);
        DefaultHttpContext context = CreateContext();
        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
        Assert.Equal(200, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ResponseContentTypeIsApplicationJson()
    {
        RequestDelegate next = _ => throw new NotFoundException("Missing");
        GlobalExceptionMiddleware middleware = CreateMiddleware(next);
        DefaultHttpContext context = CreateContext();
        await middleware.InvokeAsync(context);

        Assert.StartsWith("application/json", context.Response.ContentType);
    }
}
