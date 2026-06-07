using PollaMundialista.Domain.Entities;

namespace PollaMundialista.Application.Common.Interfaces;

/// <summary>Data-access contract for the <see cref="User"/> aggregate.</summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
}
