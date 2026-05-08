using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Parrot.Api.Middleware;
using Parrot.Application.Logging;

namespace Test.Parrot.Api.Tests.Middleware;

public class RequestContextMiddlewareTests
{
    private static RequestContextMiddleware CreateMiddleware(RequestDelegate? next = null) =>
        new RequestContextMiddleware(
            next ?? (_ => Task.CompletedTask),
            NullLogger<RequestContextMiddleware>.Instance);

    private static DefaultHttpContext CreateContext(
        string method = "GET",
        string path = "/api/test",
        string? correlationId = null,
        string? userAgent = null,
        string? remoteIp = null)
    {
        DefaultHttpContext context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();

        if (correlationId is not null)
        {
            context.Request.Headers["X-Correlation-ID"] = correlationId;
        }

        if (userAgent is not null)
        {
            context.Request.Headers["User-Agent"] = userAgent;
        }

        if (remoteIp is not null)
        {
            context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse(remoteIp);
        }

        return context;
    }

    [Fact]
    public async Task InvokeAsync_WhenValidCorrelationIdProvided_ReusesIt()
    {
        DefaultHttpContext context = CreateContext(correlationId: "valid-id-123");
        RequestContext requestContext = new RequestContext();

        await CreateMiddleware().InvokeAsync(context, requestContext);

        Assert.Equal("valid-id-123", requestContext.CorrelationId);
    }

    [Fact]
    public async Task InvokeAsync_WhenNoCorrelationIdHeader_GeneratesNewGuid()
    {
        DefaultHttpContext context = CreateContext();
        RequestContext requestContext = new RequestContext();

        await CreateMiddleware().InvokeAsync(context, requestContext);

        Assert.True(Guid.TryParse(requestContext.CorrelationId, out _),
            $"Expected a GUID but got: {requestContext.CorrelationId}");
    }

    [Fact]
    public async Task InvokeAsync_WhenCorrelationIdExceedsMaxLength_GeneratesNewGuid()
    {
        string tooLong = new string('a', 129);
        DefaultHttpContext context = CreateContext(correlationId: tooLong);
        RequestContext requestContext = new RequestContext();

        await CreateMiddleware().InvokeAsync(context, requestContext);

        Assert.True(Guid.TryParse(requestContext.CorrelationId, out _));
    }

    [Fact]
    public async Task InvokeAsync_WhenCorrelationIdContainsInvalidChars_GeneratesNewGuid()
    {
        DefaultHttpContext context = CreateContext(correlationId: "bad!@#$id");
        RequestContext requestContext = new RequestContext();

        await CreateMiddleware().InvokeAsync(context, requestContext);

        Assert.True(Guid.TryParse(requestContext.CorrelationId, out _));
    }

    [Fact]
    public async Task InvokeAsync_CorrelationIdWithDashesAndUnderscores_IsAccepted()
    {
        DefaultHttpContext context = CreateContext(correlationId: "valid-id_ABC-123");
        RequestContext requestContext = new RequestContext();

        await CreateMiddleware().InvokeAsync(context, requestContext);

        Assert.Equal("valid-id_ABC-123", requestContext.CorrelationId);
    }

    [Fact]
    public async Task InvokeAsync_PopulatesRequestContextFromRequest()
    {
        DefaultHttpContext context = CreateContext(
            method: "POST",
            path: "/api/auth/login",
            userAgent: "TestClient/2.0",
            remoteIp: "192.168.1.42");
        RequestContext requestContext = new RequestContext();

        await CreateMiddleware().InvokeAsync(context, requestContext);

        Assert.Equal("POST", requestContext.HttpMethod);
        Assert.Equal("/api/auth/login", requestContext.RequestPath);
        Assert.Equal("TestClient/2.0", requestContext.UserAgent);
        Assert.Equal("192.168.1.42", requestContext.IpAddress);
    }

    [Fact]
    public async Task InvokeAsync_EchoesCorrelationIdInResponseHeader()
    {
        DefaultHttpContext context = CreateContext(correlationId: "echo-this");
        RequestContext requestContext = new RequestContext();

        await CreateMiddleware().InvokeAsync(context, requestContext);

        Assert.Equal("echo-this", context.Response.Headers["X-Correlation-ID"].ToString());
    }

    [Fact]
    public async Task InvokeAsync_GeneratedIdIsEchoedInResponseHeader()
    {
        DefaultHttpContext context = CreateContext();
        RequestContext requestContext = new RequestContext();

        await CreateMiddleware().InvokeAsync(context, requestContext);

        string headerValue = context.Response.Headers["X-Correlation-ID"].ToString();
        Assert.Equal(requestContext.CorrelationId, headerValue);
    }

    [Fact]
    public async Task InvokeAsync_CallsNextDelegate()
    {
        bool nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        DefaultHttpContext context = CreateContext();
        RequestContext requestContext = new RequestContext();

        await CreateMiddleware(next).InvokeAsync(context, requestContext);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_WhenCorrelationIdIsEmptyString_GeneratesNewGuid()
    {
        DefaultHttpContext context = CreateContext(correlationId: string.Empty);
        RequestContext requestContext = new RequestContext();

        await CreateMiddleware().InvokeAsync(context, requestContext);

        Assert.True(Guid.TryParse(requestContext.CorrelationId, out _));
    }
}
