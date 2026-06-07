using PollaMundialista.Application.Common.Interfaces;

namespace PollaMundialista.Infrastructure.Identity;

/// <summary>BCrypt-based password hasher. Work factor is handled by the BCrypt.Net library defaults.</summary>
public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string hash)
        => BCrypt.Net.BCrypt.Verify(password, hash);
}
