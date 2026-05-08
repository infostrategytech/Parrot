using System.ComponentModel.DataAnnotations;

namespace Parrot.Application.DTOs.Auth;

public record ResendVerificationEmailRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public required string Email { get; init; }
}
