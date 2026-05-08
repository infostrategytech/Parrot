using Microsoft.AspNetCore.Identity;

namespace Parrot.Application.Models;

public class ApplicationUser : IdentityUser
{
    public required string BusinessName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
