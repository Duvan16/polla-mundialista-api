using FluentAssertions;
using Moq;
using PollaMundialista.Application.Common.Interfaces;
using PollaMundialista.Application.Features.Auth.Commands.RegisterUser;
using PollaMundialista.Domain.Entities;
using PollaMundialista.Domain.Enums;
using TokenEntity = PollaMundialista.Domain.Entities.RefreshToken;

namespace PollaMundialista.Tests.Application.Auth;

public class RegisterUserHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokens = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtService> _jwt = new();

    private RegisterUserCommandHandler CreateHandler()
    {
        _jwt.Setup(j => j.GenerateAccessToken(It.IsAny<User>()))
            .Returns(new AccessToken("jwt-token", DateTime.UtcNow.AddMinutes(15)));
        _jwt.Setup(j => j.GenerateRefreshToken())
            .Returns(new RefreshTokenMaterial("plain-refresh", "hash-refresh", DateTime.UtcNow.AddDays(14)));
        return new RegisterUserCommandHandler(
            _users.Object, _refreshTokens.Object, _uow.Object, _hasher.Object, _jwt.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithAccessAndRefreshTokens()
    {
        var command = new RegisterUserCommand("new@test.com", "Password1", "Alice");

        _users.Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
              .ReturnsAsync((User?)null);
        _hasher.Setup(h => h.Hash(command.Password)).Returns("hashed");
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Email.Should().Be(command.Email);
        result.Value.DisplayName.Should().Be(command.DisplayName);
        result.Value.Role.Should().Be(UserRole.User.ToString());
        result.Value.AccessToken.Should().Be("jwt-token");
        result.Value.RefreshToken.Should().Be("plain-refresh");

        _users.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _refreshTokens.Verify(r => r.AddAsync(It.IsAny<TokenEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsFailure()
    {
        var existing = User.Create("existing@test.com", "hash", "Bob");
        var command = new RegisterUserCommand("existing@test.com", "Password1", "Bob");

        _users.Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
              .ReturnsAsync(existing);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Email is already registered.");

        _users.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _refreshTokens.Verify(r => r.AddAsync(It.IsAny<TokenEntity>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidCommand_HashesPasswordBeforeStoring()
    {
        var command = new RegisterUserCommand("user@test.com", "PlainText1", "Charlie");
        User? capturedUser = null;

        _users.Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
              .ReturnsAsync((User?)null);
        _hasher.Setup(h => h.Hash(command.Password)).Returns("bcrypt-hash");
        _users.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
              .Callback<User, CancellationToken>((u, _) => capturedUser = u);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        await CreateHandler().Handle(command, CancellationToken.None);

        capturedUser.Should().NotBeNull();
        capturedUser!.PasswordHash.Should().Be("bcrypt-hash");
        capturedUser.PasswordHash.Should().NotBe(command.Password);
    }
}
