using AuthenticationAPI.Common.Interfaces;
using AuthenticationAPI.Common.Results;
using AuthenticationAPI.DTO.Requests;
using AuthenticationAPI.DTO.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationAPI.Controllers;

[ApiController]
[Route("v1/[controller]")]
public class AuthController(
    IAuthService authService,
    ICurrentUserService currentUserService
) : ControllerBase
{
    [HttpPost("signup")]
    public async Task<IActionResult> Signup(
        [FromBody] SignupRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await authService.Signup(request, cancellationToken);
        if (result.IsError)
            return StatusCode(result.ErrorHttpCode, result.ErrorIdentifier);

        var addJwtCookieResult = TryAddJwtInCookies(result.Value);
        if (addJwtCookieResult.IsError)
        {
            return StatusCode(result.ErrorHttpCode, result.ErrorIdentifier);
        }

        return Created();
    }

    [HttpPost("signin")]
    public async Task<IActionResult> Signin(
        [FromBody] SigninRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await authService.Signin(request, cancellationToken);
        if (result.IsError)
            return StatusCode(result.ErrorHttpCode, result.ErrorIdentifier);

        var addJwtCookieResult = TryAddJwtInCookies(result.Value);
        if (addJwtCookieResult.IsError)
        {
            return StatusCode(result.ErrorHttpCode, result.ErrorIdentifier);
        }

        return Accepted();
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        // Get refresh token from request.
        if (!Request.Cookies.TryGetValue("X-Refresh-Token", out string? refreshToken) || !Guid.TryParse(refreshToken, out Guid parsedRefreshToken))
        {
            return StatusCode(StatusCodes.Status401Unauthorized, "error.invalid_refresh_token");
        }

        // Get user to send in service method.
        var requiredUserResult = currentUserService.GetRequiredUser();
        if (requiredUserResult.IsError)
            return StatusCode(StatusCodes.Status401Unauthorized, requiredUserResult.ErrorIdentifier);

        // Send to service.
        var result = await authService.Logout(parsedRefreshToken, requiredUserResult.Value!.Id, cancellationToken);
        if (result.IsError)
            return StatusCode(result.ErrorHttpCode, result.ErrorIdentifier);

        ClearJwtTokens();

        return Accepted();
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        // Get refresh token from request.
        if (!Request.Cookies.TryGetValue("X-Refresh-Token", out string? refreshToken) || !Guid.TryParse(refreshToken, out Guid parsedRefreshToken))
        {
            return StatusCode(StatusCodes.Status401Unauthorized, "error.invalid_refresh_token");
        }

        // Send to service
        var result = await authService.Refresh(parsedRefreshToken, cancellationToken);
        if (result.IsError)
            return StatusCode(result.ErrorHttpCode, result.ErrorIdentifier);

        var addJwtCookieResult = TryAddJwtInCookies(result.Value, true);
        if (addJwtCookieResult.IsError)
        {
            return StatusCode(result.ErrorHttpCode, result.ErrorIdentifier);
        }

        return Accepted();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var requiredUserResult = currentUserService.GetRequiredUser();
        if (requiredUserResult.IsError)
            return StatusCode(requiredUserResult.ErrorHttpCode, requiredUserResult.ErrorIdentifier);

        var authenticatedUser = requiredUserResult.Value;
        if (authenticatedUser == null)
            return StatusCode(StatusCodes.Status500InternalServerError, "error.null_value");

        return Ok(authenticatedUser);
    }

    private void ClearJwtTokens()
    {
        var cookieOptions = GetCookieOptions();

        Response.Cookies.Delete("X-Access-Token", cookieOptions);
        Response.Cookies.Delete("X-Refresh-Token", cookieOptions);
    }

    private Result TryAddJwtInCookies(JWTResponse? jwtResponse, bool removedOldCookies = false)
    {
        if (jwtResponse == null)
            return Result.Failure(StatusCodes.Status500InternalServerError, "internal.null_value");

        var cookieOptions = GetCookieOptions();

        if (removedOldCookies)
        {
            ClearJwtTokens();
        }

        Response.Cookies.Append("X-Access-Token", jwtResponse.AccessToken, cookieOptions);
        Response.Cookies.Append("X-Refresh-Token", jwtResponse.RefreshToken.ToString(), cookieOptions);

        return Result.Success();
    }

    private static CookieOptions GetCookieOptions() => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Expires = DateTimeOffset.Now.AddDays(7)
    };
}
