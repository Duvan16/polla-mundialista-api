using Microsoft.EntityFrameworkCore;
using PollaMundialista.Application.Common.Interfaces;
using PollaMundialista.Domain.Entities;

namespace PollaMundialista.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        => _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

    public async Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default)
        => await _context.RefreshTokens.AddAsync(token, cancellationToken);
}
