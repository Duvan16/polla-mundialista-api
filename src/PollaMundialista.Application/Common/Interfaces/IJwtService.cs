using PollaMundialista.Domain.Entities;

namespace PollaMundialista.Application.Common.Interfaces;

/// <summary>Generates and hashes JWT access tokens and opaque refresh tokens.</summary>
public interface IJwtService
{
    /// <summary>Creates a signed JWT containing the user's identity claims.</summary>
    AccessToken GenerateAccessToken(User user);

    /// <summary>
    /// Generates a cryptographically random refresh token and returns both the plain value
    /// (to send to the client) and its hash (to store in the database).
    /// </summary>
    RefreshTokenMaterial GenerateRefreshToken();

    /// <summary>Computes the SHA-256 hex digest of a plain refresh token for database lookup.</summary>
    string HashRefreshToken(string plainToken);
}

/// <summary>A signed JWT access token and its expiry.</summary>
public record AccessToken(string Token, DateTime ExpiresAt);

/// <summary>
/// Material produced when generating a refresh token: the plain value sent to the client,
/// its hash stored in the database, and the expiry time.
/// </summary>
public record RefreshTokenMaterial(string PlainToken, string TokenHash, DateTime ExpiresAt);
