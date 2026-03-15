using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Parrot.Api.Services.Auth;
using Parrot.Api.Utils;

namespace Test.Parrot.Api.Tests.Utils;

public class JwtHelperTests
{
    private const string Secret = "super-secret-key-that-is-at-least-32-chars!!";
    private const string Issuer = "TestIssuer";
    private const string Audience = "TestAudience";
    private const int ExpirationMinutes = 60;

    private static JwtHelper CreateSut(int expirationMinutes = ExpirationMinutes) =>
        new JwtHelper(Options.Create(new JwtSettings
        {
            Secret = Secret,
            Issuer = Issuer,
            Audience = Audience,
            ExpirationInMinutes = expirationMinutes,
        }));

    private static JwtSecurityToken Decode(string token) =>
        new JwtSecurityTokenHandler().ReadJwtToken(token);

    [Fact]
    public void GenerateToken_ProducesSignedJwt_WithCorrectIssuerAndAudience()
    {
        JwtHelper sut = CreateSut();

        string token = sut.GenerateToken([], DateTime.UtcNow.AddHours(1));

        JwtSecurityToken decoded = Decode(token);
        Assert.Equal(Issuer, decoded.Issuer);
        Assert.Contains(Audience, decoded.Audiences);
    }

    [Fact]
    public void GenerateToken_IncludesAllProvidedClaims()
    {
        JwtHelper sut = CreateSut();
        Claim[] claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub, "user-abc"),
            new Claim(JwtRegisteredClaimNames.Email, "test@example.com"),
            new Claim("businessName", "Acme Corp"),
        ];

        string token = sut.GenerateToken(claims, DateTime.UtcNow.AddHours(1));

        JwtSecurityToken decoded = Decode(token);
        Assert.Contains(decoded.Claims, c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == "user-abc");
        Assert.Contains(decoded.Claims, c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "test@example.com");
        Assert.Contains(decoded.Claims, c => c.Type == "businessName" && c.Value == "Acme Corp");
    }

    [Fact]
    public void GenerateToken_UsesProvidedExpiry()
    {
        JwtHelper sut = CreateSut();
        DateTime expiry = new DateTime(2030, 6, 15, 12, 0, 0, DateTimeKind.Utc);

        string token = sut.GenerateToken([], expiry);

        JwtSecurityToken decoded = Decode(token);
        Assert.Equal(expiry, decoded.ValidTo, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void GenerateToken_WhenExpiresNotProvided_UsesDefaultExpiration()
    {
        JwtHelper sut = CreateSut(expirationMinutes: 30);
        DateTime before = DateTime.UtcNow;

        string token = sut.GenerateToken([]);

        DateTime after = DateTime.UtcNow;
        JwtSecurityToken decoded = Decode(token);
        Assert.InRange(
            decoded.ValidTo,
            before.AddMinutes(30).AddSeconds(-2),
            after.AddMinutes(30).AddSeconds(2));
    }

    [Fact]
    public void GetDefaultExpiration_ReturnsUtcNowPlusConfiguredMinutes()
    {
        JwtHelper sut = CreateSut(expirationMinutes: 45);
        DateTime before = DateTime.UtcNow;

        DateTime result = sut.GetDefaultExpiration();

        DateTime after = DateTime.UtcNow;
        Assert.InRange(result, before.AddMinutes(45).AddSeconds(-1), after.AddMinutes(45).AddSeconds(1));
    }

    [Fact]
    public void GenerateToken_ProducesUniqueTokensForDifferentClaims()
    {
        JwtHelper sut = CreateSut();
        DateTime expiry = DateTime.UtcNow.AddHours(1);

        string token1 = sut.GenerateToken([new Claim(JwtRegisteredClaimNames.Sub, "user-1")], expiry);
        string token2 = sut.GenerateToken([new Claim(JwtRegisteredClaimNames.Sub, "user-2")], expiry);

        Assert.NotEqual(token1, token2);
    }
}
