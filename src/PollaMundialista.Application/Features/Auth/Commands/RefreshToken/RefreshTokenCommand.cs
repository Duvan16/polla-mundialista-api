using MediatR;
using PollaMundialista.Application.Common;
using PollaMundialista.Application.Features.Auth.DTOs;

namespace PollaMundialista.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<AuthResponse>>;
