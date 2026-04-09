using Parrot.Web.Features.Auth.Login.Models;
using Parrot.Web.Features.Auth.Models;

namespace Parrot.Web.Features.Auth.Login.Services;

public interface ILoginService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request);
}
