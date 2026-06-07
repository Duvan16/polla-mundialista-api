using MediatR;
using Microsoft.Extensions.Logging;
using PollaMundialista.Application.Common;
using PollaMundialista.Application.Common.Interfaces;
using PollaMundialista.Application.Features.Auth.DTOs;
using PollaMundialista.Domain.Entities;
using TokenEntity = PollaMundialista.Domain.Entities.RefreshToken;

namespace PollaMundialista.Application.Features.Auth.Commands.Login;

/// <summary>Handles <see cref="LoginCommand"/>: validates credentials and issues JWT + refresh token pair.</summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtService _jwt;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUserRepository users,
        IRefreshTokenRepository refreshTokens,
        IUnitOfWork uow,
        IPasswordHasher hasher,
        IJwtService jwt,
        ICurrentUser currentUser,
        ILogger<LoginCommandHandler> logger)
    {
        _users = users;
        _refreshTokens = refreshTokens;
        _uow = uow;
        _hasher = hasher;
        _jwt = jwt;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByEmailAsync(request.Email, cancellationToken);

        // Deliberate: same message for "not found" and "wrong password" to avoid user enumeration
        if (user is null || !_hasher.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed for email {Email} from {Ip}",
                request.Email, _currentUser.IpAddress);
            return Result<AuthResponse>.Failure("Invalid email or password.");
        }

        var response = await IssueTokensAsync(user, cancellationToken);

        _logger.LogInformation("Login succeeded for user {UserId} ({Email}) from {Ip}",
            user.Id, user.Email, _currentUser.IpAddress);

        return Result<AuthResponse>.Success(response);
    }

    private async Task<AuthResponse> IssueTokensAsync(User user, CancellationToken cancellationToken)
    {
        var access = _jwt.GenerateAccessToken(user);
        var refresh = _jwt.GenerateRefreshToken();

        var entity = TokenEntity.Create(user.Id, refresh.TokenHash, refresh.ExpiresAt, DateTime.UtcNow);
        await _refreshTokens.AddAsync(entity, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            user.Id,
            user.Email,
            user.DisplayName,
            user.Role.ToString(),
            access.Token,
            access.ExpiresAt,
            refresh.PlainToken,
            refresh.ExpiresAt);
    }
}
