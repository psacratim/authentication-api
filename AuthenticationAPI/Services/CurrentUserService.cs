using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthenticationAPI.Common.Interfaces;
using AuthenticationAPI.Common.Results;
using AuthenticationAPI.Common.Results.Internal;

namespace AuthenticationAPI.Services;

public class CurrentUserService(
    IHttpContextAccessor httpContextAccessor
) : ICurrentUserService
{
    public AuthenticatedUser? GetAuthenticatedUser()
    {
        var user = httpContextAccessor?.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
            return null;

        var rawId = user.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (rawId == null || !Guid.TryParse(rawId, out Guid parsedId))
            return null;

        return new AuthenticatedUser(
            Id: parsedId,
            Email: user.FindFirstValue(JwtRegisteredClaimNames.Email) ?? string.Empty,
            FullName: user.FindFirstValue(JwtRegisteredClaimNames.Name) ?? string.Empty
        );
    }

    public Result<AuthenticatedUser> GetRequiredUser()
    {
        var authenticatedUser = GetAuthenticatedUser();
        if (authenticatedUser == null)
            return Result<AuthenticatedUser>.Failure(StatusCodes.Status401Unauthorized, "error.no_required_user");

        return Result<AuthenticatedUser>.Success(authenticatedUser);
    }
}
