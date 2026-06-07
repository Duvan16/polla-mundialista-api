namespace PollaMundialista.Application.Features.Auth.DTOs;

/// <summary>Response payload returned on successful login, registration, or token refresh.</summary>
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
