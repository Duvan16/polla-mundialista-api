using MediatR;
using PollaMundialista.Application.Common;
using PollaMundialista.Application.Features.Auth.DTOs;

namespace PollaMundialista.Application.Features.Auth.Commands.RegisterUser;

/// <summary>Creates a new user account and immediately returns a JWT access token and refresh token.</summary>
public record RegisterUserCommand(
    string Email,
    string Password,
    string DisplayName
) : IRequest<Result<AuthResponse>>;
