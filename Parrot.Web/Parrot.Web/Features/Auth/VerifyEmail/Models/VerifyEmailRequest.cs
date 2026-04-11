namespace Parrot.Web.Features.Auth.VerifyEmail.Models;

public class VerifyEmailRequest
{
    public string UserId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
