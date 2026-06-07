namespace PollaMundialista.Application.Common.Interfaces;

/// <summary>
/// Provides identity information about the authenticated user for the current HTTP request.
/// Implemented by <c>CurrentUserService</c> in the API layer via <c>IHttpContextAccessor</c>.
/// </summary>
public interface ICurrentUser
{
    Guid UserId { get; }
    string Email { get; }
    string Role { get; }
    bool IsAuthenticated { get; }

    /// <summary>Remote IP address of the request, used for security audit logging.</summary>
    string? IpAddress { get; }
}
