using MediatR;
using PollaMundialista.Application.Common;

namespace PollaMundialista.Application.Features.Auth.Commands.Logout;

public record LogoutCommand(string RefreshToken) : IRequest<Result>;
