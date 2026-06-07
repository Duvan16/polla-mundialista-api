using MediatR;
using PollaMundialista.Application.Common;
using PollaMundialista.Application.Features.Predictions.DTOs;

namespace PollaMundialista.Application.Features.Admin.Queries.GetAllMatchesAdmin;

/// <summary>Returns all matches (with actual scores where available) for the Admin dashboard.</summary>
public record GetAllMatchesAdminQuery() : IRequest<Result<IReadOnlyList<MatchWithPredictionDto>>>;
