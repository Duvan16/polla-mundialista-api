using MediatR;
using PollaMundialista.Application.Common;

namespace PollaMundialista.Application.Features.Auth.Commands.Logout;

/// <summary>Revokes the given refresh token, effectively ending the user's session.</summary>
public record LogoutCommand(string RefreshToken) : IRequest<Result>;
