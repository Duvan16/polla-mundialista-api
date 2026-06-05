using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PollaMundialista.Application.Common.Interfaces;
using PollaMundialista.Application.Features.Auth.Commands.Login;
using PollaMundialista.Domain.Entities;
using PollaMundialista.Domain.Enums;

namespace PollaMundialista.Tests.Application.Auth;

public class LoginHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokens = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtService> _jwt = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly ILogger<LoginCommandHandler> _logger = NullLogger<LoginCommandHandler>.Instance;

    private LoginCommandHandler CreateHandler()
    {
        _jwt.Setup(j => j.GenerateAccessToken(It.IsAny<User>()))
            .Returns(new AccessToken("jwt-token", DateTime.UtcNow.AddMinutes(15)));
        _jwt.Setup(j => j.GenerateRefreshToken())
            .Returns(new RefreshTokenMaterial("plain-refresh", "hash-refresh", DateTime.UtcNow.AddDays(14)));
        return new LoginCommandHandler(
            _users.Object, _refreshTokens.Object, _uow.Object,
            _hasher.Object, _jwt.Object, _currentUser.Object, _logger);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsSuccessWithAccessAndRefreshTokens()
    {
        var user = User.Create("alice@test.com", "hashed-pw", "Alice");
        var command = new LoginCommand("alice@test.com", "Password1");

        _users.Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
              .ReturnsAsync(user);
        _hasher.Setup(h => h.Verify(command.Password, user.PasswordHash)).Returns(true);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Email.Should().Be(user.Email);
        result.Value.AccessToken.Should().Be("jwt-token");
        result.Value.RefreshToken.Should().Be("plain-refresh");

        _refreshTokens.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsFailureAndDoesNotIssueTokens()
    {
        var command = new LoginCommand("ghost@test.com", "Password1");

        _users.Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
              .ReturnsAsync((User?)null);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid email or password.");

        _jwt.Verify(j => j.GenerateAccessToken(It.IsAny<User>()), Times.Never);
        _refreshTokens.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WrongPassword_ReturnsFailureAndDoesNotIssueTokens()
    {
        var user = User.Create("alice@test.com", "hashed-pw", "Alice");
        var command = new LoginCommand("alice@test.com", "WrongPass1");

        _users.Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
              .ReturnsAsync(user);
        _hasher.Setup(h => h.Verify(command.Password, user.PasswordHash)).Returns(false);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid email or password.");

        _jwt.Verify(j => j.GenerateAccessToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WrongPassword_ReturnsIdenticalErrorAsNotFound()
    {
        // Guard against user enumeration: both branches must return the same message
        var user = User.Create("alice@test.com", "hashed-pw", "Alice");

        _users.Setup(r => r.GetByEmailAsync("alice@test.com", It.IsAny<CancellationToken>()))
              .ReturnsAsync(user);
        _hasher.Setup(h => h.Verify(It.IsAny<string>(), user.PasswordHash)).Returns(false);

        _users.Setup(r => r.GetByEmailAsync("ghost@test.com", It.IsAny<CancellationToken>()))
              .ReturnsAsync((User?)null);

        var wrongPwResult = await CreateHandler().Handle(
            new LoginCommand("alice@test.com", "Bad1"), CancellationToken.None);

        var notFoundResult = await CreateHandler().Handle(
            new LoginCommand("ghost@test.com", "Bad1"), CancellationToken.None);

        wrongPwResult.Error.Should().Be(notFoundResult.Error);
    }

    [Fact]
    public async Task Handle_AdminUser_ReturnsAdminRole()
    {
        var admin = User.Create("admin@test.com", "hashed-pw", "Admin", UserRole.Admin);
        var command = new LoginCommand("admin@test.com", "Password1");

        _users.Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
              .ReturnsAsync(admin);
        _hasher.Setup(h => h.Verify(command.Password, admin.PasswordHash)).Returns(true);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Role.Should().Be(UserRole.Admin.ToString());
    }
}
