using FluentAssertions;
using Moq;
using PollaMundialista.Application.Common.Interfaces;
using PollaMundialista.Application.Features.Auth.Commands.Login;
using PollaMundialista.Domain.Entities;
using PollaMundialista.Domain.Enums;

namespace PollaMundialista.Tests.Application.Auth;

public class LoginHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtService> _jwt = new();

    private LoginCommandHandler CreateHandler() =>
        new(_users.Object, _hasher.Object, _jwt.Object);

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsSuccessWithToken()
    {
        // Arrange
        var user = User.Create("alice@test.com", "hashed-pw", "Alice");
        var command = new LoginCommand("alice@test.com", "Password1");

        _users.Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
              .ReturnsAsync(user);
        _hasher.Setup(h => h.Verify(command.Password, user.PasswordHash)).Returns(true);
        _jwt.Setup(j => j.GenerateToken(user)).Returns("jwt-token");

        // Act
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Email.Should().Be(user.Email);
        result.Value.Token.Should().Be("jwt-token");
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsFailure()
    {
        // Arrange
        var command = new LoginCommand("ghost@test.com", "Password1");

        _users.Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
              .ReturnsAsync((User?)null);

        // Act
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid email or password.");

        _jwt.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WrongPassword_ReturnsFailure()
    {
        // Arrange
        var user = User.Create("alice@test.com", "hashed-pw", "Alice");
        var command = new LoginCommand("alice@test.com", "WrongPass1");

        _users.Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
              .ReturnsAsync(user);
        _hasher.Setup(h => h.Verify(command.Password, user.PasswordHash)).Returns(false);

        // Act
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid email or password.");

        _jwt.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
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
        // Arrange
        var admin = User.Create("admin@test.com", "hashed-pw", "Admin", UserRole.Admin);
        var command = new LoginCommand("admin@test.com", "Password1");

        _users.Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
              .ReturnsAsync(admin);
        _hasher.Setup(h => h.Verify(command.Password, admin.PasswordHash)).Returns(true);
        _jwt.Setup(j => j.GenerateToken(admin)).Returns("admin-token");

        // Act
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Role.Should().Be(UserRole.Admin.ToString());
    }
}
