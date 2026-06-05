using FluentAssertions;
using Moq;
using PollaMundialista.Application.Common.Interfaces;
using PollaMundialista.Application.Features.Auth.Commands.Logout;
using TokenEntity = PollaMundialista.Domain.Entities.RefreshToken;

namespace PollaMundialista.Tests.Application.Auth;

public class LogoutHandlerTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokens = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IJwtService> _jwt = new();

    private LogoutCommandHandler CreateHandler()
    {
        _jwt.Setup(j => j.HashRefreshToken(It.IsAny<string>()))
            .Returns<string>(t => $"hash:{t}");
        return new LogoutCommandHandler(_refreshTokens.Object, _uow.Object, _jwt.Object);
    }

    [Fact]
    public async Task Handle_ActiveToken_RevokesAndSaves()
    {
        var stored = TokenEntity.Create(Guid.NewGuid(), "hash:r1", DateTime.UtcNow.AddDays(1), DateTime.UtcNow);
        _refreshTokens.Setup(r => r.GetByHashAsync("hash:r1", It.IsAny<CancellationToken>()))
                      .ReturnsAsync(stored);

        var result = await CreateHandler().Handle(new LogoutCommand("r1"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        stored.RevokedAt.Should().NotBeNull();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UnknownToken_ReturnsSuccessSilently()
    {
        _refreshTokens.Setup(r => r.GetByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync((TokenEntity?)null);

        var result = await CreateHandler().Handle(new LogoutCommand("ghost"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AlreadyRevokedToken_NoOp()
    {
        var stored = TokenEntity.Create(Guid.NewGuid(), "hash:r1", DateTime.UtcNow.AddDays(1), DateTime.UtcNow);
        var firstRevokeTime = DateTime.UtcNow.AddMinutes(-5);
        stored.Revoke(firstRevokeTime);

        _refreshTokens.Setup(r => r.GetByHashAsync("hash:r1", It.IsAny<CancellationToken>()))
                      .ReturnsAsync(stored);

        var result = await CreateHandler().Handle(new LogoutCommand("r1"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        stored.RevokedAt.Should().BeCloseTo(firstRevokeTime, TimeSpan.FromMilliseconds(1));
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
