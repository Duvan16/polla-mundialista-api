using FluentAssertions;
using Moq;
using PollaMundialista.Application.Common.Interfaces;
using PollaMundialista.Application.Features.Auth.Commands.RegisterUser;
using PollaMundialista.Domain.Entities;
using PollaMundialista.Domain.Enums;

namespace PollaMundialista.Tests.Application.Auth;

public class RegisterUserHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtService> _jwt = new();

    private RegisterUserCommandHandler CreateHandler() =>
        new(_users.Object, _uow.Object, _hasher.Object, _jwt.Object);

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithToken()
    {
        // Arrange
        var command = new RegisterUserCommand("new@test.com", "Password1", "Alice");

        _users.Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
              .ReturnsAsync((User?)null);
        _hasher.Setup(h => h.Hash(command.Password)).Returns("hashed");
        _jwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("jwt-token");
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Email.Should().Be(command.Email);
        result.Value.DisplayName.Should().Be(command.DisplayName);
        result.Value.Role.Should().Be(UserRole.User.ToString());
        result.Value.Token.Should().Be("jwt-token");

        _users.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsFailure()
    {
        // Arrange
        var existing = User.Create("existing@test.com", "hash", "Bob");
        var command = new RegisterUserCommand("existing@test.com", "Password1", "Bob");

        _users.Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
              .ReturnsAsync(existing);

        // Act
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Email is already registered.");

        _users.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidCommand_HashesPasswordBeforeStoring()
    {
        // Arrange
        var command = new RegisterUserCommand("user@test.com", "PlainText1", "Charlie");
        User? capturedUser = null;

        _users.Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
              .ReturnsAsync((User?)null);
        _hasher.Setup(h => h.Hash(command.Password)).Returns("bcrypt-hash");
        _users.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
              .Callback<User, CancellationToken>((u, _) => capturedUser = u);
        _jwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("token");
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser!.PasswordHash.Should().Be("bcrypt-hash");
        capturedUser.PasswordHash.Should().NotBe(command.Password);
    }
}
