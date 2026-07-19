using AuthenticationAPI.Common.Results;

namespace AuthenticationAPI.Extensions;

public static class ResultExtensions
{
    public static Result Validate<T>(this Result<T> result)
    {
        if (result.IsError)
            return Result.Failure(result.ErrorHttpCode, result.ErrorIdentifier);

        if (result.Value == null)
            return Result.Failure("error.null_value_in_result");

        return Result.Success();
    }
}
