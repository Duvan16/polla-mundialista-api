using MediatR;
using PollaMundialista.Application.Common;
using PollaMundialista.Application.Features.Predictions.DTOs;

namespace PollaMundialista.Application.Features.Admin.Queries.GetAllMatchesAdmin;

public record GetAllMatchesAdminQuery() : IRequest<Result<IReadOnlyList<MatchWithPredictionDto>>>;
