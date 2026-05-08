using Parrot.Application.DTOs.Auth;

namespace Parrot.Application.Auth;

public interface IAuthService
{
    Task RegisterAsync(RegisterRequest request);

    Task<AuthResponse> LoginAsync(LoginRequest request);

    Task<AuthResponse> VerifyEmailAsync(VerifyEmailRequest request);

    Task ResendVerificationEmailAsync(ResendVerificationEmailRequest request);

    Task ForgotPasswordAsync(ForgotPasswordRequest request);

    Task ResetPasswordAsync(ResetPasswordRequest request);

    Task ChangePasswordAsync(string userId, ChangePasswordRequest request);
}
