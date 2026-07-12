namespace AuthenticationAPI.Common.Results;

public class Result
{
    public bool IsError { get; set; }
    public int ErrorHttpCode { get; set; } = StatusCodes.Status500InternalServerError;
    public string ErrorIdentifier { get; set; } = "no_identifier";

    public static Result Success() => new()
    {
        IsError = false
    };

    public static Result Failure(int httpCode, string errorIdentifier = "no_identifier") => new()
    {
        IsError = true,
        ErrorHttpCode = httpCode,
        ErrorIdentifier = errorIdentifier
    };
}
