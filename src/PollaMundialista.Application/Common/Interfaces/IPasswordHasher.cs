namespace PollaMundialista.Application.Common.Interfaces;

/// <summary>Abstracts password hashing so the algorithm (BCrypt) stays in Infrastructure.</summary>
public interface IPasswordHasher
{
    /// <summary>Returns a salted hash of the plain-text password.</summary>
    string Hash(string password);

    /// <summary>Returns true if <paramref name="password"/> matches the stored <paramref name="hash"/>.</summary>
    bool Verify(string password, string hash);
}
