using System.Net.Http.Json;
using Parrot.Web.Features.Auth.Models;
using Parrot.Web.Features.Auth.VerifyEmail.Models;

namespace Parrot.Web.Features.Auth.VerifyEmail.Services;

public class VerifyEmailService : IVerifyEmailService
{
    private readonly HttpClient _httpClient;

    public VerifyEmailService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AuthResponse?> VerifyEmailAsync(VerifyEmailRequest request)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/auth/verify-email", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AuthResponse>();
    }

    public async Task ResendVerificationEmailAsync(ResendVerificationEmailRequest request)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/auth/resend-verification", request);
        response.EnsureSuccessStatusCode();
    }
}
