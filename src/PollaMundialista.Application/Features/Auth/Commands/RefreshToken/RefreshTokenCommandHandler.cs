using MediatR;
using PollaMundialista.Application.Common;
using PollaMundialista.Application.Common.Interfaces;
using PollaMundialista.Application.Features.Auth.DTOs;
using TokenEntity = PollaMundialista.Domain.Entities.RefreshToken;

namespace PollaMundialista.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// Handles <see cref="RefreshTokenCommand"/>: validates and rotates the refresh token,
/// revoking the old one and issuing a fresh pair to prevent token reuse.
/// </summary>
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
{
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;
    private readonly IJwtService _jwt;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokens,
        IUserRepository users,
        IUnitOfWork uow,
        IJwtService jwt)
    {
        _refreshTokens = refreshTokens;
        _users = users;
        _uow = uow;
        _jwt = jwt;
    }

    public async Task<Result<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var hash = _jwt.HashRefreshToken(request.RefreshToken);
        var stored = await _refreshTokens.GetByHashAsync(hash, cancellationToken);

        var now = DateTime.UtcNow;
        if (stored is null || !stored.IsActive(now))
            return Result<AuthResponse>.Failure("Invalid refresh token.");

        var user = await _users.GetByIdAsync(stored.UserId, cancellationToken);
        if (user is null)
            return Result<AuthResponse>.Failure("Invalid refresh token.");

        var access = _jwt.GenerateAccessToken(user);
        var newRefresh = _jwt.GenerateRefreshToken();
        var newEntity = TokenEntity.Create(user.Id, newRefresh.TokenHash, newRefresh.ExpiresAt, now);

        stored.Revoke(now, newEntity.Id);
        await _refreshTokens.AddAsync(newEntity, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return Result<AuthResponse>.Success(new AuthResponse(
            user.Id,
            user.Email,
            user.DisplayName,
            user.Role.ToString(),
            access.Token,
            access.ExpiresAt,
            newRefresh.PlainToken,
            newRefresh.ExpiresAt));
    }
}
