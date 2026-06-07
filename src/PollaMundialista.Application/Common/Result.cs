namespace PollaMundialista.Application.Common;

/// <summary>
/// Represents the outcome of an operation that returns a value, using the Result pattern
/// instead of exceptions for expected business failures.
/// </summary>
/// <typeparam name="T">The type of the success value.</typeparam>
public class Result<T>
{
    public bool IsSuccess { get; }

    /// <summary>The success payload. Only populated when <see cref="IsSuccess"/> is true.</summary>
    public T? Value { get; }

    /// <summary>A human-readable error message. Only populated when <see cref="IsSuccess"/> is false.</summary>
    public string? Error { get; }

    private Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);
}

/// <summary>
/// Represents the outcome of a void operation, using the Result pattern instead of exceptions
/// for expected business failures.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }

    /// <summary>A human-readable error message. Only populated when <see cref="IsSuccess"/> is false.</summary>
    public string? Error { get; }

    private Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);
}
