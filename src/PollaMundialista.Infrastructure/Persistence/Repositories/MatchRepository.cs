using Microsoft.EntityFrameworkCore;
using PollaMundialista.Application.Common.Interfaces;
using PollaMundialista.Domain.Entities;

namespace PollaMundialista.Infrastructure.Persistence.Repositories;

public class MatchRepository : IMatchRepository
{
    private readonly AppDbContext _context;

    public MatchRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Match?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Matches.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Match>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Matches.OrderBy(m => m.MatchDate).ToListAsync(cancellationToken);

    public async Task AddAsync(Match match, CancellationToken cancellationToken = default)
        => await _context.Matches.AddAsync(match, cancellationToken);
}
