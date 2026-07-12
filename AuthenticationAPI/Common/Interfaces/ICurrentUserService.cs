using AuthenticationAPI.Common.Results;
using AuthenticationAPI.Common.Results.Internal;

namespace AuthenticationAPI.Common.Interfaces;

public interface ICurrentUserService
{
    public AuthenticatedUser? GetAuthenticatedUser();
    public Result<AuthenticatedUser> GetRequiredUser();
}
