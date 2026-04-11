using System.Net.Http.Json;
using Parrot.Web.Features.Auth.ForgotPassword.Models;

namespace Parrot.Web.Features.Auth.ForgotPassword.Services;

public class ForgotPasswordService : IForgotPasswordService
{
    private readonly HttpClient _httpClient;

    public ForgotPasswordService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/auth/forgot-password", request);
        response.EnsureSuccessStatusCode();
    }
}
