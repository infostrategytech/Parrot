namespace Parrot.Web.Features.Auth.ForgotPassword.State;

public class ForgotPasswordState
{
    public bool IsLoading { get; private set; }
    public string? ErrorMessage { get; private set; }
    public bool IsSubmitted { get; private set; }

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

    public void SetSubmitted()
    {
        IsSubmitted = true;
        NotifyStateChanged();
    }

    public void Reset()
    {
        IsLoading = false;
        ErrorMessage = null;
        IsSubmitted = false;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
