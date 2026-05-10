using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Parrot.Web.Features.Auth.Models;

namespace Parrot.Web.Services;

public class TokenStorageService
{
    private const string StorageKey = "auth";
    private readonly ProtectedLocalStorage _localStorage;

    public TokenStorageService(ProtectedLocalStorage localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task SaveAsync(AuthResponse auth)
    {
        await _localStorage.SetAsync(StorageKey, auth);
    }

    public async Task<AuthResponse?> LoadAsync()
    {
        try
        {
            ProtectedBrowserStorageResult<AuthResponse> result =
                await _localStorage.GetAsync<AuthResponse>(StorageKey);

            if (!result.Success || result.Value is null)
                return null;

            // Discard expired tokens
            if (result.Value.ExpiresAt <= DateTime.UtcNow)
            {
                await ClearAsync();
                return null;
            }

            return result.Value;
        }
        catch
        {
            return null;
        }
    }

    public async Task ClearAsync()
    {
        await _localStorage.DeleteAsync(StorageKey);
    }
}
