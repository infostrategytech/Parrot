namespace Parrot.Api.DTOs.Auth;

public record AuthResponse(
    string Token,
    DateTime ExpiresAt,
    string UserId,
    string Email,
    string BusinessName
);
