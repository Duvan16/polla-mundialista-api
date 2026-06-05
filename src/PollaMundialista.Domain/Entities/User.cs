using PollaMundialista.Domain.Enums;

namespace PollaMundialista.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string DisplayName { get; private set; } = default!;
    public UserRole Role { get; private set; }

    public ICollection<Prediction> Predictions { get; private set; } = new List<Prediction>();

    private User() { }

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
