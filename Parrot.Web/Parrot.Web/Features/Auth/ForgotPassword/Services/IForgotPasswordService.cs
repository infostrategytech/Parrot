using Parrot.Web.Features.Auth.ForgotPassword.Models;

namespace Parrot.Web.Features.Auth.ForgotPassword.Services;

public interface IForgotPasswordService
{
    Task ForgotPasswordAsync(ForgotPasswordRequest request);
}
