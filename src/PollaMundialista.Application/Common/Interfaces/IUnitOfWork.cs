namespace PollaMundialista.Application.Common.Interfaces;

/// <summary>
/// Flushes all pending EF Core change-tracker entries to the database in a single transaction.
/// Keeps the Application layer decoupled from <c>DbContext</c>.
/// </summary>
public interface IUnitOfWork
{
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
