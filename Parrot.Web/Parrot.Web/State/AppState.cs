namespace Parrot.Web.State;

public class AppState
{
    public event Action? OnChange;

    private void NotifyStateChanged() => OnChange?.Invoke();
}
