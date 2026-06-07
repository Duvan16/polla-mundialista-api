using MediatR;
using PollaMundialista.Application.Common;
using PollaMundialista.Application.Common.Interfaces;
using PollaMundialista.Application.Features.Auth.DTOs;
using PollaMundialista.Domain.Entities;
using TokenEntity = PollaMundialista.Domain.Entities.RefreshToken;

namespace PollaMundialista.Application.Features.Auth.Commands.RegisterUser;

/// <summary>Handles <see cref="RegisterUserCommand"/>: creates the user (rejecting duplicate emails) and issues tokens.</summary>
public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtService _jwt;

    public RegisterUserCommandHandler(
        IUserRepository users,
        IRefreshTokenRepository refreshTokens,
        IUnitOfWork uow,
        IPasswordHasher hasher,
        IJwtService jwt)
    {
        _users = users;
        _refreshTokens = refreshTokens;
        _uow = uow;
        _hasher = hasher;
        _jwt = jwt;
    }

    public async Task<Result<AuthResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existing = await _users.GetByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
            return Result<AuthResponse>.Failure("Email is already registered.");

        var user = User.Create(request.Email, _hasher.Hash(request.Password), request.DisplayName);
        await _users.AddAsync(user, cancellationToken);

        var access = _jwt.GenerateAccessToken(user);
        var refresh = _jwt.GenerateRefreshToken();
        var token = TokenEntity.Create(user.Id, refresh.TokenHash, refresh.ExpiresAt, DateTime.UtcNow);
        await _refreshTokens.AddAsync(token, cancellationToken);

        await _uow.SaveChangesAsync(cancellationToken);

        return Result<AuthResponse>.Success(new AuthResponse(
            user.Id,
            user.Email,
            user.DisplayName,
            user.Role.ToString(),
            access.Token,
            access.ExpiresAt,
            refresh.PlainToken,
            refresh.ExpiresAt));
    }
}
