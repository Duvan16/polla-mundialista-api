using MediatR;
using PollaMundialista.Application.Common;
using PollaMundialista.Application.Common.Interfaces;

namespace PollaMundialista.Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUnitOfWork _uow;
    private readonly IJwtService _jwt;

    public LogoutCommandHandler(
        IRefreshTokenRepository refreshTokens,
        IUnitOfWork uow,
        IJwtService jwt)
    {
        _refreshTokens = refreshTokens;
        _uow = uow;
        _jwt = jwt;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var hash = _jwt.HashRefreshToken(request.RefreshToken);
        var stored = await _refreshTokens.GetByHashAsync(hash, cancellationToken);

        if (stored is not null && stored.RevokedAt is null)
        {
            stored.Revoke(DateTime.UtcNow);
            await _uow.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
