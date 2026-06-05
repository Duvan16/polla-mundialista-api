using PollaMundialista.Domain.Entities;

namespace PollaMundialista.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}
