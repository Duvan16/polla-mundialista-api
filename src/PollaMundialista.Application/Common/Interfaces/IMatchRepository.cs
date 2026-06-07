using PollaMundialista.Domain.Entities;

namespace PollaMundialista.Application.Common.Interfaces;

/// <summary>Data-access contract for the <see cref="Match"/> aggregate.</summary>
public interface IMatchRepository
{
    Task<Match?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Match>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Match match, CancellationToken cancellationToken = default);
}
