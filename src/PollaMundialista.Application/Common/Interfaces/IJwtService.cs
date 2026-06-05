using PollaMundialista.Domain.Entities;

namespace PollaMundialista.Application.Common.Interfaces;

public interface IJwtService
{
    AccessToken GenerateAccessToken(User user);
    RefreshTokenMaterial GenerateRefreshToken();
    string HashRefreshToken(string plainToken);
}

public record AccessToken(string Token, DateTime ExpiresAt);

public record RefreshTokenMaterial(string PlainToken, string TokenHash, DateTime ExpiresAt);
