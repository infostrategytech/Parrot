using System.Net.Http.Json;
using Parrot.Web.Features.Auth.Models;
using Parrot.Web.Features.Auth.Register.Models;

namespace Parrot.Web.Features.Auth.Register.Services;

public class RegisterService : IRegisterService
{
    private readonly HttpClient _httpClient;

    public RegisterService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
        response.EnsureSuccessStatusCode();
        // API returns 201 Created with no body — registration requires email verification before login
        return null;
    }
}
