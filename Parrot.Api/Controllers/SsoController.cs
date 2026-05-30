using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Parrot.Application.Auth;
using Parrot.Application.DTOs.Auth;
using Parrot.Api.Middleware;

namespace Parrot.Api.Controllers;

[ApiController]
[Route("api/auth/sso")]
public class SsoController : ControllerBase
{
    private const string ExternalCookieScheme = "ExternalCookies";

    private readonly IAuthService _authService;
    private readonly FrontendSettings _frontendSettings;

    public SsoController(IAuthService authService, IOptions<FrontendSettings> frontendSettings)
    {
        _authService = authService;
        _frontendSettings = frontendSettings.Value;
    }

    [HttpGet("google")]
    public IActionResult InitiateGoogle()
    {
        AuthenticationProperties properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(GoogleCallback), "Sso"),
        };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("google/callback")]
    public async Task<IActionResult> GoogleCallback()
    {
        AuthenticateResult result = await HttpContext.AuthenticateAsync(ExternalCookieScheme);

        if (!result.Succeeded || result.Principal is null)
        {
            return Redirect($"{_frontendSettings.BaseUrl}/login?error=sso_failed");
        }

        string? email = result.Principal.FindFirstValue(ClaimTypes.Email);
        string? name = result.Principal.FindFirstValue(ClaimTypes.Name) ?? email;
        string? googleId = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(googleId))
        {
            return Redirect($"{_frontendSettings.BaseUrl}/login?error=sso_missing_claims");
        }

        await HttpContext.SignOutAsync(ExternalCookieScheme);

        AuthResponse authResponse = await _authService.ExternalLoginAsync(email, name ?? email, "Google", googleId);

        string redirectUrl = string.Format(
            "{0}/sso-callback?token={1}&expiresAt={2}&userId={3}&email={4}&businessName={5}",
            _frontendSettings.BaseUrl,
            Uri.EscapeDataString(authResponse.Token),
            Uri.EscapeDataString(authResponse.ExpiresAt.ToString("O")),
            Uri.EscapeDataString(authResponse.UserId),
            Uri.EscapeDataString(authResponse.Email),
            Uri.EscapeDataString(authResponse.BusinessName));

        return Redirect(redirectUrl);
    }
}
