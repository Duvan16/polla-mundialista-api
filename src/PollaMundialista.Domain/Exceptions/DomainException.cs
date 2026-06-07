namespace PollaMundialista.Domain.Exceptions;

/// <summary>
/// Thrown when a domain invariant is violated (e.g. setting a result on an already-finished match).
/// Caught by <c>ExceptionMiddleware</c> and returned as HTTP 400.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
