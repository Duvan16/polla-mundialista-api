namespace PollaMundialista.Infrastructure.Identity;

/// <summary>Strongly-typed options bound from the <c>JwtSettings</c> section of appsettings.json.</summary>
public class JwtSettings
{
    public string SecretKey { get; init; } = default!;
    public string Issuer { get; init; } = default!;
    public string Audience { get; init; } = default!;
    public int ExpirationMinutes { get; init; } = 15;
    public int RefreshTokenDays { get; init; } = 14;
}
