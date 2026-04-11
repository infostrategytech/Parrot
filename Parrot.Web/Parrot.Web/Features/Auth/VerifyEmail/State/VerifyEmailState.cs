namespace Parrot.Web.Features.Auth.VerifyEmail.State;

public class VerifyEmailState
{
    public bool IsLoading { get; private set; }
    public string? ErrorMessage { get; private set; }
    public bool IsVerified { get; private set; }

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

    public void SetVerified()
    {
        IsVerified = true;
        NotifyStateChanged();
    }

    public void Reset()
    {
        IsLoading = false;
        ErrorMessage = null;
        IsVerified = false;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
