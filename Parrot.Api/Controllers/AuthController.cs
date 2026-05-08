using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Parrot.Application.Auth;
using Parrot.Application.DTOs.Auth;
using Parrot.Api.Middleware;
using Parrot.Api.Utils;

namespace Parrot.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [EnableRateLimiting(RateLimitPolicies.Register)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request)
    {
        await _authService.RegisterAsync(request);
        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpPost("login")]
    [EnableRateLimiting(RateLimitPolicies.Login)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AuthResponse>> LoginAsync([FromBody] LoginRequest request)
    {
        AuthResponse response = await _authService.LoginAsync(request);
        return Ok(response);
    }

    [HttpPost("verify-email")]
    [EnableRateLimiting(RateLimitPolicies.VerifyEmail)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AuthResponse>> VerifyEmailAsync([FromBody] VerifyEmailRequest request)
    {
        AuthResponse response = await _authService.VerifyEmailAsync(request);
        return Ok(response);
    }

    [HttpPost("resend-verification")]
    [EnableRateLimiting(RateLimitPolicies.ResendVerification)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ResendVerificationEmailAsync([FromBody] ResendVerificationEmailRequest request)
    {
        await _authService.ResendVerificationEmailAsync(request);
        return NoContent();
    }

    [HttpPost("forgot-password")]
    [EnableRateLimiting(RateLimitPolicies.ForgotPassword)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordRequest request)
    {
        await _authService.ForgotPasswordAsync(request);
        return NoContent();
    }

    [HttpPost("reset-password")]
    [EnableRateLimiting(RateLimitPolicies.ResetPassword)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest request)
    {
        await _authService.ResetPasswordAsync(request);
        return NoContent();
    }

    [Authorize]
    [HttpPost("change-password")]
    [EnableRateLimiting(RateLimitPolicies.ChangePassword)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordRequest request)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (userId is null)
        {
            return Unauthorized();
        }

        await _authService.ChangePasswordAsync(userId, request);
        return NoContent();
    }
}
