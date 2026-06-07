using PollaMundialista.Domain.Entities;

namespace PollaMundialista.Application.Common.Interfaces;

/// <summary>Data-access contract for the <see cref="Prediction"/> aggregate.</summary>
public interface IPredictionRepository
{
    Task<Prediction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Enforces the one-prediction-per-user-per-match invariant by looking up an existing entry.</summary>
    Task<Prediction?> GetByUserAndMatchAsync(Guid userId, Guid matchId, CancellationToken cancellationToken = default);

    /// <summary>Returns all predictions for a match. Used during bulk point recalculation after a result is set.</summary>
    Task<IReadOnlyList<Prediction>> GetByMatchIdAsync(Guid matchId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Prediction>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Returns all predictions with their associated User navigation property populated, used for leaderboard aggregation.</summary>
    Task<IReadOnlyList<Prediction>> GetAllWithUsersAsync(CancellationToken cancellationToken = default);

    Task AddAsync(Prediction prediction, CancellationToken cancellationToken = default);
}
