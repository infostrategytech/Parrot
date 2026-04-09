using Parrot.Web.Features.Auth.Models;
using Parrot.Web.Features.Auth.Register.Models;

namespace Parrot.Web.Features.Auth.Register.Services;

public interface IRegisterService
{
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
}
