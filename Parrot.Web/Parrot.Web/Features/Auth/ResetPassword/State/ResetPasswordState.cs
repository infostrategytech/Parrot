namespace Parrot.Web.Features.Auth.ResetPassword.State;

public class ResetPasswordState
{
    public bool IsLoading { get; private set; }
    public string? ErrorMessage { get; private set; }
    public bool IsSuccess { get; private set; }

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

    public void SetSuccess()
    {
        IsSuccess = true;
        NotifyStateChanged();
    }

    public void Reset()
    {
        IsLoading = false;
        ErrorMessage = null;
        IsSuccess = false;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
