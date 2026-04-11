using Parrot.Web.Features.Auth.Models;

namespace Parrot.Web.Features.Auth.State;

public class AuthState
{
    public AuthResponse? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser is not null;

    public event Action? OnChange;

    public void SetUser(AuthResponse user)
    {
        CurrentUser = user;
        NotifyStateChanged();
    }

    public void ClearUser()
    {
        CurrentUser = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
