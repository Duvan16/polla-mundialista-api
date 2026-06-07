using PollaMundialista.Domain.Entities;

namespace PollaMundialista.Application.Common.Interfaces;

/// <summary>Data-access contract for <see cref="RefreshToken"/> lookup and persistence.</summary>
public interface IRefreshTokenRepository
{
    /// <summary>Looks up a token by its SHA-256 hash (the plain token is never stored).</summary>
    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default);
}
