using AuthenticationAPI.Common.Interfaces;
using AuthenticationAPI.Common.Results;
using AuthenticationAPI.Common.Results.Internal;
using AuthenticationAPI.Common.Types;
using AuthenticationAPI.DTO.Requests;
using AuthenticationAPI.Extensions;

namespace AuthenticationAPI.Services;

public class AuthService(
    ISessionService sessionService,
    IAccountService accountService
) : IAuthService
{
    public async Task<Result<UserSession>> Signup(SignupRequest request, CancellationToken cancellationToken = default)
    {
        // Create a new account
        var accountResult = await accountService.CreateAccount(request.Email, request.FullName, request.Password, cancellationToken);
        var accountValidation = accountResult.Validate();
        if (accountValidation.IsError)
            return Result<UserSession>.Failure(accountValidation.ErrorHttpCode, accountValidation.ErrorIdentifier);

        // Create a new access and refresh token to return back has cookie.
        var account = accountResult.Value!;
        var userSessionResult = await sessionService.CreateSession(account, cancellationToken);
        var sessionValidation = userSessionResult.Validate();
        if (sessionValidation.IsError)
            return Result<UserSession>.Failure(sessionValidation.ErrorHttpCode, sessionValidation.ErrorIdentifier);

        return Result<UserSession>.Success(userSessionResult.Value!);
    }

    public async Task<Result<UserSession>> Signin(SigninRequest request, AuthenticatedUser? user, CancellationToken cancellationToken = default)
    {
        if (user != null && user.Email != request.Email)
            return Result<UserSession>.Failure(StatusCodes.Status403Forbidden, "error.already_logged_in_another_account");

        var account = await accountService.GetAccountByEmail(request.Email, cancellationToken);
        if (account == null)
            return Result<UserSession>.Failure(StatusCodes.Status401Unauthorized, "error.invalid_credentials");

        var passwordValidation = await accountService.ValidatePassword(account, request.Password, cancellationToken);
        if (passwordValidation.IsError)
            return Result<UserSession>.Failure(passwordValidation.ErrorHttpCode, passwordValidation.ErrorIdentifier);

        var userSessionResult = await sessionService.CreateSession(account, cancellationToken);
        var sessionValidation = userSessionResult.Validate();
        if (sessionValidation.IsError)
            return Result<UserSession>.Failure(sessionValidation.ErrorHttpCode, sessionValidation.ErrorIdentifier);

        return Result<UserSession>.Success(userSessionResult.Value!);
    }

    public async Task<Result> Logout(Guid refreshToken, Guid userId, CancellationToken cancellationToken = default)
    {
        var session = await sessionService.GetSessionByRefreshToken(refreshToken, cancellationToken);
        if (session == null || session.AccountId != userId)
            return Result.Failure(StatusCodes.Status401Unauthorized, "error.invalid_session");

        var invalidateResult = await sessionService.RevokeSession(session, cancellationToken);
        if (invalidateResult.IsError)
            return invalidateResult;

        return Result.Success();
    }

    public async Task<Result<UserSession>> Refresh(Guid refreshToken, CancellationToken cancellationToken = default)
    {
        var accountSession = await sessionService.GetSessionByRefreshToken(refreshToken, cancellationToken);
        var refreshResult = await sessionService.RefreshSession(accountSession, cancellationToken);
        var refreshValidation = refreshResult.Validate();
        if (refreshValidation.IsError)
            return Result<UserSession>.Failure(refreshValidation.ErrorHttpCode, refreshValidation.ErrorIdentifier);

        return Result<UserSession>.Success(refreshResult.Value!);
    }

}
