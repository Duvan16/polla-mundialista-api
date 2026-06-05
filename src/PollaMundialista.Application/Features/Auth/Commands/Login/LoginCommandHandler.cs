using MediatR;
using PollaMundialista.Application.Common;
using PollaMundialista.Application.Common.Interfaces;
using PollaMundialista.Application.Features.Auth.DTOs;

namespace PollaMundialista.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtService _jwt;

    public LoginCommandHandler(
        IUserRepository users,
        IPasswordHasher hasher,
        IJwtService jwt)
    {
        _users = users;
        _hasher = hasher;
        _jwt = jwt;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByEmailAsync(request.Email, cancellationToken);

        // Deliberate: same message for "not found" and "wrong password" to avoid user enumeration
        if (user is null || !_hasher.Verify(request.Password, user.PasswordHash))
            return Result<AuthResponse>.Failure("Invalid email or password.");

        var token = _jwt.GenerateToken(user);

        return Result<AuthResponse>.Success(new AuthResponse(
            user.Id,
            user.Email,
            user.DisplayName,
            user.Role.ToString(),
            token));
    }
}
