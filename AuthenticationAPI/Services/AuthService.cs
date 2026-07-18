using AuthenticationAPI.Common;
using AuthenticationAPI.Common.Interfaces;
using AuthenticationAPI.Common.Results;
using AuthenticationAPI.Common.Results.Internal;
using AuthenticationAPI.Common.Types;
using AuthenticationAPI.Database;
using AuthenticationAPI.Database.Entities;
using AuthenticationAPI.DTO.Requests;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationAPI.Services;

public class AuthService(
    AppDbContext db,
    ISessionService sessionService,
    IAccountService accountService
) : IAuthService
{

    public async Task<Result<UserSession>> Signup(SignupRequest request, CancellationToken cancellationToken = default)
    {
        // Create a new account
        var accountResult = await accountService.CreateAccount(request.Email, request.FullName, request.Password, cancellationToken);
        var accountValidation = Utilities.ValidateResult<Account>(accountResult);
        if (accountValidation.IsError)
            return Result<UserSession>.Failure(accountValidation.ErrorHttpCode, accountValidation.ErrorIdentifier);

        // Create a new access and refresh token to return back has cookie.
        var account = accountResult.Value!;
        var userSessionResult = await sessionService.CreateSession(account, "Unknown", cancellationToken);
        var sessionValidation = Utilities.ValidateResult<UserSession>(userSessionResult);
        if (sessionValidation.IsError)
            return Result<UserSession>.Failure(sessionValidation.ErrorHttpCode, sessionValidation.ErrorIdentifier);

        return Result<UserSession>.Success(userSessionResult.Value!);
    }

    public async Task<Result<UserSession>> Signin(SigninRequest request, CancellationToken cancellationToken = default)
    {
        var account = await accountService.GetAccountByEmail(request.Email, cancellationToken);
        if (account == null)
            return Result<UserSession>.Failure(StatusCodes.Status401Unauthorized, "error.invalid_credentials");

        var passwordValidation = await accountService.ValidatePassword(account, request.Password, cancellationToken);
        if (passwordValidation.IsError)
            return Result<UserSession>.Failure(passwordValidation.ErrorHttpCode, passwordValidation.ErrorIdentifier);

        var userSessionResult = await sessionService.CreateSession(account, "Unknown", cancellationToken);
        var sessionValidation = Utilities.ValidateResult<UserSession>(userSessionResult);
        if (sessionValidation.IsError)
            return Result<UserSession>.Failure(sessionValidation.ErrorHttpCode, sessionValidation.ErrorIdentifier);

        return Result<UserSession>.Success(userSessionResult.Value!);
    }


    public async Task<Result> Logout(Guid refreshToken, Guid userId, CancellationToken cancellationToken = default)
    {
        var session = await sessionService.GetSessionByRefreshToken(refreshToken, cancellationToken);
        if (session == null || session.AccountId != userId)
            return Result.Failure(StatusCodes.Status401Unauthorized, "error.invalid_session");

        session.IsRevoked = true;
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<UserSession>> Refresh(Guid refreshToken, CancellationToken cancellationToken = default)
    {
        var accountSession = await sessionService.GetSessionByRefreshToken(refreshToken, cancellationToken);
        var refreshResult = await sessionService.RefreshSession(accountSession, cancellationToken);
        var refreshValidation = Utilities.ValidateResult<UserSession>(refreshResult);
        if (refreshValidation.IsError)
            return Result<UserSession>.Failure(refreshValidation.ErrorHttpCode, refreshValidation.ErrorIdentifier);

        return Result<UserSession>.Success(refreshResult.Value!);
    }

}
