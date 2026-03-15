using System.ComponentModel.DataAnnotations;

namespace Parrot.Api.DTOs.Auth;

public record ResetPasswordRequest
{
    [Required(ErrorMessage = "User ID is required")]
    required public string UserId { get; init; }

    [Required(ErrorMessage = "Token is required")]
    required public string Token { get; init; }

    [Required(ErrorMessage = "New password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
    required public string NewPassword { get; init; }

    [Required(ErrorMessage = "Confirm password is required")]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
    required public string ConfirmNewPassword { get; init; }
}
