using System.ComponentModel.DataAnnotations;

namespace Parrot.Application.DTOs.Auth;

public record VerifyEmailRequest
{
    [Required(ErrorMessage = "User ID is required")]
    public required string UserId { get; init; }

    [Required(ErrorMessage = "Token is required")]
    public required string Token { get; init; }
}
