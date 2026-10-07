namespace Prepstack.Domain.Common;

public readonly struct Result
{
    public bool IsSuccess { get; }

    public Error? Error { get; }

    private Result(Error? error)
    {
        IsSuccess = error is null;
        Error = error;
    }

    public static Result Success() => new(error: null);

    public static Result Failure(Error error) => new(error);

    public static implicit operator Result(Error error) => Failure(error);
}

public readonly struct Result<T>
{
    private readonly T? _value;

    public bool IsSuccess { get; }

    public Error? Error { get; }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on a failed result.");

    private Result(T value)
    {
        _value = value;
        IsSuccess = true;
        Error = null;
    }

    private Result(Error error)
    {
        _value = default;
        IsSuccess = false;
        Error = error;
    }

    public static Result<T> Success(T value) => new(value);

    public static Result<T> Failure(Error error) => new(error);

    public static implicit operator Result<T>(Error error) => Failure(error);
}
