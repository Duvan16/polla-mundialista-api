using MediatR;
using PollaMundialista.Application.Common;
using PollaMundialista.Application.Features.Auth.DTOs;

namespace PollaMundialista.Application.Features.Auth.Commands.Login;

/// <summary>Authenticates a user with email and password, returning a JWT access token and refresh token.</summary>
public record LoginCommand(
    string Email,
    string Password
) : IRequest<Result<AuthResponse>>;
