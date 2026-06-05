using FluentAssertions;
using Moq;
using PollaMundialista.Application.Common.Interfaces;
using PollaMundialista.Application.Features.Auth.Commands.RefreshToken;
using PollaMundialista.Domain.Entities;
using TokenEntity = PollaMundialista.Domain.Entities.RefreshToken;

namespace PollaMundialista.Tests.Application.Auth;

public class RefreshTokenHandlerTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokens = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IJwtService> _jwt = new();

    private RefreshTokenCommandHandler CreateHandler()
    {
        _jwt.Setup(j => j.HashRefreshToken(It.IsAny<string>()))
            .Returns<string>(t => $"hash:{t}");
        _jwt.Setup(j => j.GenerateAccessToken(It.IsAny<User>()))
            .Returns(new AccessToken("new-access", DateTime.UtcNow.AddMinutes(15)));
        _jwt.Setup(j => j.GenerateRefreshToken())
            .Returns(new RefreshTokenMaterial("new-plain-refresh", "new-hash-refresh", DateTime.UtcNow.AddDays(14)));
        return new RefreshTokenCommandHandler(
            _refreshTokens.Object, _users.Object, _uow.Object, _jwt.Object);
    }

    [Fact]
    public async Task Handle_ValidRefreshToken_RotatesAndReturnsNewTokens()
    {
        var user = User.Create("alice@test.com", "hash", "Alice");
        var stored = TokenEntity.Create(user.Id, "hash:r1", DateTime.UtcNow.AddDays(1), DateTime.UtcNow);

        _refreshTokens.Setup(r => r.GetByHashAsync("hash:r1", It.IsAny<CancellationToken>()))
                      .ReturnsAsync(stored);
        _users.Setup(u => u.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
              .ReturnsAsync(user);

        var result = await CreateHandler().Handle(new RefreshTokenCommand("r1"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("new-access");
        result.Value.RefreshToken.Should().Be("new-plain-refresh");

        stored.RevokedAt.Should().NotBeNull();
        stored.ReplacedByTokenId.Should().NotBeNull();
        _refreshTokens.Verify(r => r.AddAsync(It.IsAny<TokenEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UnknownToken_ReturnsFailure()
    {
        _refreshTokens.Setup(r => r.GetByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync((TokenEntity?)null);

        var result = await CreateHandler().Handle(new RefreshTokenCommand("ghost"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid refresh token.");
        _refreshTokens.Verify(r => r.AddAsync(It.IsAny<TokenEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ExpiredToken_ReturnsFailureAndDoesNotRotate()
    {
        var user = User.Create("alice@test.com", "hash", "Alice");
        var expired = TokenEntity.Create(user.Id, "hash:r1", DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(-15));

        _refreshTokens.Setup(r => r.GetByHashAsync("hash:r1", It.IsAny<CancellationToken>()))
                      .ReturnsAsync(expired);

        var result = await CreateHandler().Handle(new RefreshTokenCommand("r1"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid refresh token.");
        _refreshTokens.Verify(r => r.AddAsync(It.IsAny<TokenEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RevokedToken_Rejected_ReplayAttackBlocked()
    {
        var user = User.Create("alice@test.com", "hash", "Alice");
        var revoked = TokenEntity.Create(user.Id, "hash:r1", DateTime.UtcNow.AddDays(1), DateTime.UtcNow);
        revoked.Revoke(DateTime.UtcNow, Guid.NewGuid());

        _refreshTokens.Setup(r => r.GetByHashAsync("hash:r1", It.IsAny<CancellationToken>()))
                      .ReturnsAsync(revoked);

        var result = await CreateHandler().Handle(new RefreshTokenCommand("r1"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid refresh token.");
        _refreshTokens.Verify(r => r.AddAsync(It.IsAny<TokenEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RotatesToken_MarksOldRevokedWithReplacedById()
    {
        var user = User.Create("alice@test.com", "hash", "Alice");
        var stored = TokenEntity.Create(user.Id, "hash:r1", DateTime.UtcNow.AddDays(1), DateTime.UtcNow);

        TokenEntity? captured = null;
        _refreshTokens.Setup(r => r.GetByHashAsync("hash:r1", It.IsAny<CancellationToken>()))
                      .ReturnsAsync(stored);
        _users.Setup(u => u.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
              .ReturnsAsync(user);
        _refreshTokens.Setup(r => r.AddAsync(It.IsAny<TokenEntity>(), It.IsAny<CancellationToken>()))
                      .Callback<TokenEntity, CancellationToken>((t, _) => captured = t);

        await CreateHandler().Handle(new RefreshTokenCommand("r1"), CancellationToken.None);

        captured.Should().NotBeNull();
        stored.ReplacedByTokenId.Should().Be(captured!.Id);
    }

    [Fact]
    public async Task Handle_UserMissing_ReturnsFailure()
    {
        var stored = TokenEntity.Create(Guid.NewGuid(), "hash:r1", DateTime.UtcNow.AddDays(1), DateTime.UtcNow);
        _refreshTokens.Setup(r => r.GetByHashAsync("hash:r1", It.IsAny<CancellationToken>()))
                      .ReturnsAsync(stored);
        _users.Setup(u => u.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync((User?)null);

        var result = await CreateHandler().Handle(new RefreshTokenCommand("r1"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid refresh token.");
    }
}
