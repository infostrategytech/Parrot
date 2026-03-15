using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Parrot.Api.Services.Auth;

namespace Parrot.Api.Utils;

public interface IJwtHelper
{
    string GenerateToken(IEnumerable<Claim> claims, DateTime? expires = null);

    DateTime GetDefaultExpiration();
}

public class JwtHelper : IJwtHelper
{
    private readonly JwtSettings _jwtSettings;

    public JwtHelper(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public string GenerateToken(IEnumerable<Claim> claims, DateTime? expires = null)
    {
        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        DateTime expiry = expires ?? GetDefaultExpiration();

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiry,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public DateTime GetDefaultExpiration() =>
        DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes);
}
