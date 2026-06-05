using MediatR;
using PollaMundialista.Application.Common;
using PollaMundialista.Application.Features.Auth.DTOs;

namespace PollaMundialista.Application.Features.Auth.Commands.Login;

public record LoginCommand(
    string Email,
    string Password
) : IRequest<Result<AuthResponse>>;
