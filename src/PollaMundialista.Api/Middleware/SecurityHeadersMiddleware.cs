namespace PollaMundialista.Api.Middleware;

/// <summary>
/// Adds defensive HTTP security headers (X-Content-Type-Options, X-Frame-Options, Referrer-Policy)
/// to every response. Auth endpoints additionally receive <c>Cache-Control: no-store</c>.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "no-referrer";

        if (context.Request.Path.StartsWithSegments("/api/auth"))
            headers["Cache-Control"] = "no-store";

        return _next(context);
    }
}
