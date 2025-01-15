namespace SharedKernel;

public class Result<T>
{
    public T? Value { get; init; }
    public Error? Error { get; init; }
    public bool IsSuccess => Error is null;

    private Result(T value)
    {
        Value = value;
        Error = null;
    }
    
    private Result(Error error)
    {
        Value = default;
        Error = error;
    }

    public static Result<T> Success(T value)
    {
        return new Result<T>(value);
    }

    public static Result<T> Failure(Error error)
    {
        return new Result<T>(error);
    }
    
    public static implicit operator Result<T>(Error error)
    {
        return new Result<T>(error);
    }

    public static implicit operator Result<T>(T value)
    {
        return new Result<T>(value);
    }
}