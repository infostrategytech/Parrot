using System.Security.Claims;

namespace Parrot.Application.Utils;

public interface IJwtHelper
{
    string GenerateToken(IEnumerable<Claim> claims, DateTime? expires = null);

    DateTime GetDefaultExpiration();
}
