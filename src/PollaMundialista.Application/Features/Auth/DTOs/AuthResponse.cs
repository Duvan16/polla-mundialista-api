namespace PollaMundialista.Application.Features.Auth.DTOs;

public record AuthResponse(
    Guid UserId,
    string Email,
    string DisplayName,
    string Role,
    string Token
);
