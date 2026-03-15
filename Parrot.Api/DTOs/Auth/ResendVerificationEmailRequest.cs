using System.ComponentModel.DataAnnotations;

namespace Parrot.Api.DTOs.Auth;

public record ResendVerificationEmailRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    required public string Email { get; init; }
}
