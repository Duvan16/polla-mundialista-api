using MediatR;
using PollaMundialista.Application.Common;
using PollaMundialista.Application.Common.Interfaces;
using PollaMundialista.Application.Features.Auth.DTOs;
using PollaMundialista.Domain.Entities;

namespace PollaMundialista.Application.Features.Auth.Commands.RegisterUser;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtService _jwt;

    public RegisterUserCommandHandler(
        IUserRepository users,
        IUnitOfWork uow,
        IPasswordHasher hasher,
        IJwtService jwt)
    {
        _users = users;
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
        await _uow.SaveChangesAsync(cancellationToken);

        var token = _jwt.GenerateToken(user);

        return Result<AuthResponse>.Success(new AuthResponse(
            user.Id,
            user.Email,
            user.DisplayName,
            user.Role.ToString(),
            token));
    }
}
