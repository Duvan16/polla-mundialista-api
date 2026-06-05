namespace PollaMundialista.Infrastructure.Identity;

public class JwtSettings
{
    public string SecretKey { get; init; } = default!;
    public string Issuer { get; init; } = default!;
    public string Audience { get; init; } = default!;
    public int ExpirationMinutes { get; init; } = 60;
}
