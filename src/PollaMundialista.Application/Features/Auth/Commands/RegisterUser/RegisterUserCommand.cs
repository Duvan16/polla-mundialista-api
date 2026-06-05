using MediatR;
using PollaMundialista.Application.Common;
using PollaMundialista.Application.Features.Auth.DTOs;

namespace PollaMundialista.Application.Features.Auth.Commands.RegisterUser;

public record RegisterUserCommand(
    string Email,
    string Password,
    string DisplayName
) : IRequest<Result<AuthResponse>>;
