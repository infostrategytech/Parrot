using Parrot.Api.DTOs.Auth;

namespace Parrot.Api.Services.Auth;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);

    Task<AuthResponse> LoginAsync(LoginRequest request);

    Task<AuthResponse> VerifyEmailAsync(VerifyEmailRequest request);

    Task ResendVerificationEmailAsync(ResendVerificationEmailRequest request);

    Task ForgotPasswordAsync(ForgotPasswordRequest request);

    Task ResetPasswordAsync(ResetPasswordRequest request);

    Task ChangePasswordAsync(string userId, ChangePasswordRequest request);
}
