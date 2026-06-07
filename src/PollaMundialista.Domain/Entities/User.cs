using PollaMundialista.Domain.Enums;

namespace PollaMundialista.Domain.Entities;

/// <summary>
/// A registered participant in the prediction pool, with an assigned role of User or Admin.
/// </summary>
public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string DisplayName { get; private set; } = default!;
    public UserRole Role { get; private set; }

    public ICollection<Prediction> Predictions { get; private set; } = new List<Prediction>();
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

    private User() { }

    /// <summary>Creates a new user with a pre-hashed password. Defaults to the <see cref="UserRole.User"/> role.</summary>
    public static User Create(string email, string passwordHash, string displayName, UserRole role = UserRole.User)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash,
            DisplayName = displayName,
            Role = role
        };
    }
}
