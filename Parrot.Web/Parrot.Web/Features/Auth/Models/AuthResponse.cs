namespace Parrot.Web.Features.Auth.Models;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
}
