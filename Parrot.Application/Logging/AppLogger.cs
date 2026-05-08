using Microsoft.Extensions.Logging;

namespace Parrot.Application.Logging;

public class AppLogger<T> : IAppLogger<T>
{
    private readonly ILogger<T> _logger;

    public AppLogger(ILogger<T> logger)
    {
        _logger = logger;
    }

    public void LogDebug(string message, params object?[] args) =>
        _logger.LogDebug(message, args);

    public void LogInformation(string message, params object?[] args) =>
        _logger.LogInformation(message, args);

    public void LogWarning(string message, params object?[] args) =>
        _logger.LogWarning(message, args);

    public void LogWarning(Exception exception, string message, params object?[] args) =>
        _logger.LogWarning(exception, message, args);

    public void LogError(string message, params object?[] args) =>
        _logger.LogError(message, args);

    public void LogError(Exception exception, string message, params object?[] args) =>
        _logger.LogError(exception, message, args);

    public IDisposable BeginEntityScope(string entityType, string entityId) =>
        _logger.BeginScope(new Dictionary<string, object>
        {
            ["EntityType"] = entityType,
            ["EntityId"] = entityId,
        }) !;
}
