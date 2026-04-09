namespace Parrot.Web.Features.Auth.Login.State;

public class LoginState
{
    public bool IsLoading { get; private set; }
    public string? ErrorMessage { get; private set; }

    public event Action? OnChange;

    public void SetLoading(bool loading)
    {
        IsLoading = loading;
        NotifyStateChanged();
    }

    public void SetError(string? message)
    {
        ErrorMessage = message;
        NotifyStateChanged();
    }

    public void Reset()
    {
        IsLoading = false;
        ErrorMessage = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
