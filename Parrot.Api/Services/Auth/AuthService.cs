using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Parrot.Api.DTOs.Auth;
using Parrot.Api.Logging;
using Parrot.Api.Models;
using Parrot.Api.Services.Email;
using Parrot.Api.Utils;
using Parrot.Domain.Exceptions;

namespace Parrot.Api.Services.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtHelper _jwtHelper;
    private readonly IEmailService _emailService;
    private readonly EmailSettings _emailSettings;
    private readonly IAppLogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IJwtHelper jwtHelper,
        IEmailService emailService,
        IOptions<EmailSettings> emailSettings,
        IAppLogger<AuthService> logger)
    {
        _userManager = userManager;
        _jwtHelper = jwtHelper;
        _emailService = emailService;
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        ApplicationUser? user = await _userManager.FindByEmailAsync(request.Email);

        if (user is not null && await _userManager.IsLockedOutAsync(user))
        {
            using (_logger.BeginEntityScope("User", user.Id))
            {
                _logger.LogWarning("Login blocked — account is locked out. Email: {Email}", request.Email);
            }

            throw new UnauthorizedException("Account is temporarily locked due to too many failed login attempts. Please try again later.");
        }

        bool validCredentials = user is not null && await _userManager.CheckPasswordAsync(user, request.Password);

        if (!validCredentials)
        {
            if (user is not null)
            {
                await _userManager.AccessFailedAsync(user);
                using (_logger.BeginEntityScope("User", user.Id))
                {
                    _logger.LogWarning("Failed login attempt for {Email}", request.Email);
                }
            }
            else
            {
                _logger.LogWarning("Login attempt for unrecognised email");
            }

            throw new UnauthorizedException("Invalid email or password.");
        }

        if (!user!.EmailConfirmed)
        {
            using (_logger.BeginEntityScope("User", user.Id))
            {
                _logger.LogWarning("Login attempt before email verification. Email: {Email}", request.Email);
            }

            throw new UnauthorizedException("Please verify your email address before logging in.");
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        using (_logger.BeginEntityScope("User", user.Id))
        {
            _logger.LogInformation("User logged in successfully. Email: {Email}", request.Email);
        }

        return BuildAuthResponse(user);
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        ApplicationUser? existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            _logger.LogWarning("Registration attempt with already-registered email {Email}", request.Email);
            throw new ConflictException("A user with this email already exists.");
        }

        ApplicationUser user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            BusinessName = request.BusinessName,
        };

        IdentityResult result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            _logger.LogError("Identity user creation failed for {Email}", request.Email);
            throw new ValidationException("An error occurred while creating the user.");
        }

        await SendVerificationEmailAsync(user);

        using (_logger.BeginEntityScope("User", user.Id))
        {
            _logger.LogInformation("User registered and verification email sent. Email: {Email}", request.Email);
        }

        return BuildAuthResponse(user);
    }

    public async Task<AuthResponse> VerifyEmailAsync(VerifyEmailRequest request)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
        {
            _logger.LogWarning("Email verification attempt for unknown UserId: {UserId}", request.UserId);
            throw new NotFoundException("User not found.");
        }

        IdentityResult result = await _userManager.ConfirmEmailAsync(user, request.Token);
        if (!result.Succeeded)
        {
            using (_logger.BeginEntityScope("User", user.Id))
            {
                _logger.LogWarning("Email verification failed — invalid or expired token. Email: {Email}", user.Email);
            }

            throw new ValidationException("Invalid or expired verification token.");
        }

        using (_logger.BeginEntityScope("User", user.Id))
        {
            _logger.LogInformation("Email verified successfully. Email: {Email}", user.Email);
        }

        return BuildAuthResponse(user);
    }

    public async Task ResendVerificationEmailAsync(ResendVerificationEmailRequest request)
    {
        ApplicationUser? user = await _userManager.FindByEmailAsync(request.Email);

        // Return silently if user does not exist or is already confirmed to avoid email enumeration
        if (user is null || user.EmailConfirmed)
        {
            return;
        }

        await SendVerificationEmailAsync(user);

        using (_logger.BeginEntityScope("User", user.Id))
        {
            _logger.LogInformation("Verification email resent. Email: {Email}", request.Email);
        }
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        ApplicationUser? user = await _userManager.FindByEmailAsync(request.Email);

        // Return silently when user not found to prevent email enumeration
        if (user is null)
        {
            return;
        }

        string rawToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        string encodedToken = Uri.EscapeDataString(rawToken);
        string resetUrl = $"{_emailSettings.BaseUrl}/reset-password?userId={user.Id}&token={encodedToken}";

        string subject = "Reset your Parrot password";
        string body = $"""
            <h2>Password Reset</h2>
            <p>We received a request to reset your Parrot account password.</p>
            <p><a href="{resetUrl}">Reset Password</a></p>
            <p>This link expires after use. If you did not request a password reset, you can safely ignore this email.</p>
            """;

        await _emailService.SendEmailAsync(user.Email!, subject, body);

        using (_logger.BeginEntityScope("User", user.Id))
        {
            _logger.LogInformation("Password reset email sent. Email: {Email}", request.Email);
        }
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
        {
            _logger.LogWarning("Password reset attempt for unknown UserId: {UserId}", request.UserId);
            throw new NotFoundException("User not found.");
        }

        IdentityResult result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            using (_logger.BeginEntityScope("User", user.Id))
            {
                _logger.LogWarning("Password reset failed — invalid or expired token. Email: {Email}", user.Email);
            }

            throw new ValidationException("Invalid or expired password reset token.");
        }

        using (_logger.BeginEntityScope("User", user.Id))
        {
            _logger.LogInformation("Password reset successfully. Email: {Email}", user.Email);
        }
    }

    public async Task ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            _logger.LogWarning("Change password attempt for unknown UserId: {UserId}", userId);
            throw new NotFoundException("User not found.");
        }

        IdentityResult result = await _userManager.ChangePasswordAsync(
            user, request.CurrentPassword, request.NewPassword);

        if (!result.Succeeded)
        {
            using (_logger.BeginEntityScope("User", user.Id))
            {
                _logger.LogWarning("Change password failed — incorrect current password. Email: {Email}", user.Email);
            }

            throw new ValidationException("Current password is incorrect.");
        }

        using (_logger.BeginEntityScope("User", user.Id))
        {
            _logger.LogInformation("Password changed successfully. Email: {Email}", user.Email);
        }
    }

    private async Task SendVerificationEmailAsync(ApplicationUser user)
    {
        string rawToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        string encodedToken = Uri.EscapeDataString(rawToken);
        string verificationUrl = $"{_emailSettings.BaseUrl}/verify-email?userId={user.Id}&token={encodedToken}";

        string subject = "Verify your Parrot account";
        string body = $"""
            <h2>Welcome to Parrot!</h2>
            <p>Please verify your email address by clicking the link below:</p>
            <p><a href="{verificationUrl}">Verify Email</a></p>
            <p>This link will expire after use. If you did not create an account, you can safely ignore this email.</p>
            """;

        await _emailService.SendEmailAsync(user.Email!, subject, body);
    }

    private AuthResponse BuildAuthResponse(ApplicationUser user)
    {
        Claim[] claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("businessName", user.BusinessName),
        ];

        DateTime expiresAt = _jwtHelper.GetDefaultExpiration();
        string token = _jwtHelper.GenerateToken(claims, expiresAt);

        return new AuthResponse(token, expiresAt, user.Id, user.Email!, user.BusinessName);
    }
}
