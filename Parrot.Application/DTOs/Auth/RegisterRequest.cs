using System.ComponentModel.DataAnnotations;

namespace Parrot.Application.DTOs.Auth;

public record RegisterRequest
{
    [Required(ErrorMessage = "Business name is required")]
    [StringLength(256, MinimumLength = 2, ErrorMessage = "Business name must be between 2 and 256 characters")]
    public required string BusinessName { get; init; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public required string Email { get; init; }

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
    public required string Password { get; init; }

    [Required(ErrorMessage = "Confirm password is required")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
    public required string ConfirmPassword { get; init; }
}
