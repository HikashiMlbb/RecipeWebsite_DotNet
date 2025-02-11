namespace SharedKernel;

public class Result
{
    private Result()
    {
        Error = null;
    }

    private Result(Error error)
    {
        Error = error;
    }

    public Error? Error { get; init; }
    public bool IsSuccess => Error is null;

    public static Result Success()
    {
        return new Result();
    }

    public static Result Failure(Error error)
    {
        return new Result(error);
    }

    public static implicit operator Result(Error error)
    {
        return new Result(error);
    }
}