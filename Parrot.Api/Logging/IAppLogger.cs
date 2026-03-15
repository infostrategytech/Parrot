namespace Parrot.Api.Logging;

public interface IAppLogger<T>
{
    void LogDebug(string message, params object?[] args);

    void LogInformation(string message, params object?[] args);

    void LogWarning(string message, params object?[] args);

    void LogWarning(Exception exception, string message, params object?[] args);

    void LogError(string message, params object?[] args);

    void LogError(Exception exception, string message, params object?[] args);

    IDisposable BeginEntityScope(string entityType, string entityId);
}
