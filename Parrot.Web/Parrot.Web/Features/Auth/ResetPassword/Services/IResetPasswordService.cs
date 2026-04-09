using Parrot.Web.Features.Auth.ResetPassword.Models;

namespace Parrot.Web.Features.Auth.ResetPassword.Services;

public interface IResetPasswordService
{
    Task ResetPasswordAsync(ResetPasswordRequest request);
}
