using FluentValidation;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using PollaMundialista.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace PollaMundialista.Api.Middleware;

/// <summary>
/// Global exception handler that maps unhandled exceptions to RFC 7807 problem-detail responses.
/// Catches <see cref="FluentValidation.ValidationException"/> (400), <see cref="Domain.Exceptions.DomainException"/> (400),
/// <see cref="UnauthorizedAccessException"/> (401), and falls back to 500 for anything else.
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly TelemetryClient _telemetryClient;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, TelemetryClient telemetryClient)
    {
        _next = next;
        _logger = logger;
        _telemetryClient = telemetryClient;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _telemetryClient.TrackException(ex);
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, errors) = exception switch
        {
            ValidationException ve => (
                HttpStatusCode.BadRequest,
                "Validation failed",
                ve.Errors.GroupBy(e => e.PropertyName)
                         .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
            ),
            DomainException => (HttpStatusCode.BadRequest, exception.Message, (Dictionary<string, string[]>?)null),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized", null),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred", null)
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var problem = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Instance = context.Request.Path
        };

        if (errors is not null)
            problem.Extensions["errors"] = errors;

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}
