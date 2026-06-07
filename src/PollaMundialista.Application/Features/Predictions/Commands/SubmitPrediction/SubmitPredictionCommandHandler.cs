using MediatR;
using PollaMundialista.Application.Common;
using PollaMundialista.Application.Common.Interfaces;
using PollaMundialista.Application.Features.Predictions.DTOs;
using PollaMundialista.Domain.Entities;

namespace PollaMundialista.Application.Features.Predictions.Commands.SubmitPrediction;

/// <summary>
/// Handles <see cref="SubmitPredictionCommand"/>: creates the prediction or updates the existing one.
/// Rejects submissions for finished matches because scoring has already been calculated.
/// </summary>
public class SubmitPredictionCommandHandler
    : IRequestHandler<SubmitPredictionCommand, Result<SubmitPredictionResponse>>
{
    private readonly IMatchRepository _matches;
    private readonly IPredictionRepository _predictions;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public SubmitPredictionCommandHandler(
        IMatchRepository matches,
        IPredictionRepository predictions,
        IUnitOfWork uow,
        ICurrentUser currentUser)
    {
        _matches = matches;
        _predictions = predictions;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<SubmitPredictionResponse>> Handle(
        SubmitPredictionCommand request,
        CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken);
        if (match is null)
            return Result<SubmitPredictionResponse>.Failure("Match not found.");

        if (match.IsFinished)
            return Result<SubmitPredictionResponse>.Failure("Cannot submit prediction for a finished match.");

        var existing = await _predictions.GetByUserAndMatchAsync(
            _currentUser.UserId, request.MatchId, cancellationToken);

        Prediction prediction;
        if (existing is not null)
        {
            existing.UpdatePrediction(request.PredictedHomeGoals, request.PredictedAwayGoals);
            prediction = existing;
        }
        else
        {
            prediction = Prediction.Create(
                _currentUser.UserId,
                request.MatchId,
                request.PredictedHomeGoals,
                request.PredictedAwayGoals);
            await _predictions.AddAsync(prediction, cancellationToken);
        }

        await _uow.SaveChangesAsync(cancellationToken);

        return Result<SubmitPredictionResponse>.Success(new SubmitPredictionResponse(
            prediction.Id,
            match.Id,
            match.HomeTeam,
            match.AwayTeam,
            prediction.PredictedHomeGoals,
            prediction.PredictedAwayGoals));
    }
}
