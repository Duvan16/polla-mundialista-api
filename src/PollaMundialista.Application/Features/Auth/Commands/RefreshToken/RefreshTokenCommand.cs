using MediatR;
using PollaMundialista.Application.Common;
using PollaMundialista.Application.Features.Auth.DTOs;

namespace PollaMundialista.Application.Features.Auth.Commands.RefreshToken;

/// <summary>Exchanges a valid refresh token for a new access token and rotated refresh token.</summary>
public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<AuthResponse>>;
