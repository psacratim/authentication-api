namespace AuthenticationAPI.Common.Results;

public class Result<T> : Result
{
    public T? Value { get; set; }

    public static Result<T> Success(T value) => new()
    {
        IsError = false,
        Value = value
    };

    public new static Result<T> Failure(int httpCode, string errorIdentifier = "no_identifier") => new()
    {
        IsError = true,
        ErrorHttpCode = httpCode,
        ErrorIdentifier = errorIdentifier
    };

    public new static Result<T> Failure(string errorIdentifier = "no_identifier")
    {
        return Failure(StatusCodes.Status500InternalServerError, errorIdentifier);
    }
}
