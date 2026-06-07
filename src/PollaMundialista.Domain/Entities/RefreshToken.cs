namespace PollaMundialista.Domain.Entities;

/// <summary>
/// A hashed refresh token used to issue new access tokens without re-authentication.
/// </summary>
/// <remarks>
/// Only the SHA-256 hash of the token is stored, so a stolen database cannot be used to
/// replay tokens. The plain token is transmitted to the client only at issuance time.
/// </remarks>
public class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }

    /// <summary>SHA-256 hex digest of the plain token sent to the client.</summary>
    public string TokenHash { get; private set; } = default!;
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    /// <summary>Set when the token is revoked (logout or rotation). Null means still active.</summary>
    public DateTime? RevokedAt { get; private set; }

    /// <summary>ID of the token that replaced this one during rotation, for audit purposes.</summary>
    public Guid? ReplacedByTokenId { get; private set; }

    public User User { get; private set; } = default!;

    private RefreshToken() { }

    /// <summary>Creates a new refresh token record.</summary>
    /// <param name="now">The current UTC time, injected so callers control the clock.</param>
    public static RefreshToken Create(Guid userId, string tokenHash, DateTime expiresAt, DateTime now)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            CreatedAt = now
        };
    }

    /// <summary>Returns true if the token has not been revoked and has not expired.</summary>
    public bool IsActive(DateTime now) => RevokedAt is null && now < ExpiresAt;

    /// <summary>Marks this token as revoked. Idempotent — safe to call if already revoked.</summary>
    /// <param name="replacedByTokenId">ID of the successor token, when rotating rather than logging out.</param>
    public void Revoke(DateTime now, Guid? replacedByTokenId = null)
    {
        if (RevokedAt is not null) return;
        RevokedAt = now;
        ReplacedByTokenId = replacedByTokenId;
    }
}
