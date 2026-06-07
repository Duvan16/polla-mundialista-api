using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using PollaMundialista.Application.Common.Interfaces;

namespace PollaMundialista.Api.Middleware;

/// <summary>
/// Resolves the current user's identity from the JWT claims in the active <see cref="HttpContext"/>.
/// Registered as a scoped service so handlers receive per-request identity automatically.
/// </summary>
public class CurrentUserService : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public Guid UserId
    {
        get
        {
            var value = Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }

    public string Email => Principal?.FindFirstValue(JwtRegisteredClaimNames.Email) ?? string.Empty;
    public string Role => Principal?.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;
    public string? IpAddress => _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}
