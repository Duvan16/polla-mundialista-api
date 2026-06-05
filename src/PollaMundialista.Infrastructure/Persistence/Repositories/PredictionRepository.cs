using Microsoft.EntityFrameworkCore;
using PollaMundialista.Application.Common.Interfaces;
using PollaMundialista.Domain.Entities;

namespace PollaMundialista.Infrastructure.Persistence.Repositories;

public class PredictionRepository : IPredictionRepository
{
    private readonly AppDbContext _context;

    public PredictionRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Prediction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Predictions.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Prediction?> GetByUserAndMatchAsync(Guid userId, Guid matchId, CancellationToken cancellationToken = default)
        => _context.Predictions.FirstOrDefaultAsync(p => p.UserId == userId && p.MatchId == matchId, cancellationToken);

    public async Task<IReadOnlyList<Prediction>> GetByMatchIdAsync(Guid matchId, CancellationToken cancellationToken = default)
        => await _context.Predictions.Where(p => p.MatchId == matchId).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Prediction>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.Predictions.Where(p => p.UserId == userId).ToListAsync(cancellationToken);

    public async Task AddAsync(Prediction prediction, CancellationToken cancellationToken = default)
        => await _context.Predictions.AddAsync(prediction, cancellationToken);
}
