using Parrot.Web.Features.Auth.Models;
using Parrot.Web.Features.Auth.VerifyEmail.Models;

namespace Parrot.Web.Features.Auth.VerifyEmail.Services;

public interface IVerifyEmailService
{
    Task<AuthResponse?> VerifyEmailAsync(VerifyEmailRequest request);
    Task ResendVerificationEmailAsync(ResendVerificationEmailRequest request);
}
