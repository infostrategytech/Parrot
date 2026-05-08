using System.ComponentModel.DataAnnotations;

namespace Parrot.Application.DTOs.Auth;

public record LoginRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    required public string Email { get; init; }

    [Required(ErrorMessage = "Password is required")]
    required public string Password { get; init; }
}
