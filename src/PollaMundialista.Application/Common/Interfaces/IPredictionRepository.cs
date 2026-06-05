using PollaMundialista.Domain.Entities;

namespace PollaMundialista.Application.Common.Interfaces;

public interface IPredictionRepository
{
    Task<Prediction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Prediction?> GetByUserAndMatchAsync(Guid userId, Guid matchId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Prediction>> GetByMatchIdAsync(Guid matchId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Prediction>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Prediction prediction, CancellationToken cancellationToken = default);
}
