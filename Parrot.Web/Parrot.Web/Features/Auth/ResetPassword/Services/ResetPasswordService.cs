using System.Net.Http.Json;
using Parrot.Web.Features.Auth.ResetPassword.Models;

namespace Parrot.Web.Features.Auth.ResetPassword.Services;

public class ResetPasswordService : IResetPasswordService
{
    private readonly HttpClient _httpClient;

    public ResetPasswordService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/auth/reset-password", request);
        response.EnsureSuccessStatusCode();
    }
}
