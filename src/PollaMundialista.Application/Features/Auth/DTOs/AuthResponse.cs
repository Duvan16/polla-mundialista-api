namespace PollaMundialista.Application.Features.Auth.DTOs;

public record AuthResponse(
    Guid UserId,
    string Email,
    string DisplayName,
    string Role,
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt
);
