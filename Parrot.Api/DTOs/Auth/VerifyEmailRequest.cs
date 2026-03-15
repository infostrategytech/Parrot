using System.ComponentModel.DataAnnotations;

namespace Parrot.Api.DTOs.Auth;

public record VerifyEmailRequest
{
    [Required(ErrorMessage = "User ID is required")]
    required public string UserId { get; init; }

    [Required(ErrorMessage = "Token is required")]
    required public string Token { get; init; }
}
