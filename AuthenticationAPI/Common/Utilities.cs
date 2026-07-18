using AuthenticationAPI.Common.Results;

namespace AuthenticationAPI.Common;

public class Utilities
{
    public static Result ValidateResult<T>(Result<T> result)
    {
        if (result.IsError)
            return Result.Failure(result.ErrorHttpCode, result.ErrorIdentifier);

        if (result.Value == null)
            return Result.Failure("error.null_value_in_result");

        return Result.Success();
    }
}
