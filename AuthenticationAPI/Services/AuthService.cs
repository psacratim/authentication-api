using AuthenticationAPI.Common.Enums;
using AuthenticationAPI.Common.Interfaces;
using AuthenticationAPI.Common.Results;
using AuthenticationAPI.Database;
using AuthenticationAPI.Database.Entities;
using AuthenticationAPI.DTO.Requests;
using AuthenticationAPI.DTO.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationAPI.Services;

public class AuthService(
    AppDbContext db,
    IPasswordHasher<Account> passwordHasher,
    IJWTService jwtService
) : IAuthService
{

    public async Task<Result<JWTResponse>> Signup(SignupRequest request, CancellationToken cancellationToken = default)
    {
        return request.AuthMethod switch
        {
            AuthMethod.PASSWORD => await SignupWithPassword(request, cancellationToken),
            AuthMethod.OAUTH => await SignupWithOAuth(request, cancellationToken),
            _ => Result<JWTResponse>.Failure(StatusCodes.Status400BadRequest, "error.invalid_auth_method")
        };
    }

    public async Task<Result<JWTResponse>> Signin(SigninRequest request, CancellationToken cancellationToken = default)
    {
        var account = await db.Accounts.FirstOrDefaultAsync(acc => acc.Email == request.Email, cancellationToken);
        if (account == null)
            return Result<JWTResponse>.Failure(StatusCodes.Status401Unauthorized, "error.invalid_credentials");

        if (string.IsNullOrWhiteSpace(request.Password))
            return Result<JWTResponse>.Failure(StatusCodes.Status401Unauthorized, "error.invalid_credentials");

        if (account.PasswordHash == null)
            return Result<JWTResponse>.Failure(StatusCodes.Status401Unauthorized, "error.using_password_on_oauth");

        if (passwordHasher.VerifyHashedPassword(account, account.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
        {
            return Result<JWTResponse>.Failure(StatusCodes.Status401Unauthorized, "error.invalid_credentials");
        }

        var generatedJwtResult = jwtService.GenerateNewJWT(account);
        if (generatedJwtResult.IsError)
            return Result<JWTResponse>.Failure(generatedJwtResult.ErrorHttpCode, generatedJwtResult.ErrorIdentifier);

        var jwtTokens = generatedJwtResult.Value;
        if (jwtTokens == null)
            return Result<JWTResponse>.Failure(StatusCodes.Status500InternalServerError, "error.jwt_generated_null");

        var createdSession = new AccountSession
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,

            Device = "Unknown",
            RefreshToken = jwtTokens.RefreshToken,

            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await db.AddAsync(createdSession, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return Result<JWTResponse>.Success(jwtTokens);
    }

    public async Task<Result> Logout(Guid refreshToken, Guid userId, CancellationToken cancellationToken = default)
    {
        var session = await db.AccountSessions.FirstOrDefaultAsync(sess => sess.RefreshToken == refreshToken, cancellationToken);
        if (session == null)
            return Result.Failure(StatusCodes.Status401Unauthorized, "error.invalid_session");

        if (session.AccountId != userId)
            return Result.Failure(StatusCodes.Status401Unauthorized, "error.invalid_session2");

        session.IsRevoked = true;
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<JWTResponse>> Refresh(Guid refreshToken, CancellationToken cancellationToken = default)
    {
        var session = await db.AccountSessions.Include(sess => sess.Account).FirstOrDefaultAsync(sess => sess.RefreshToken == refreshToken, cancellationToken);
        if (session == null || DateTime.UtcNow > session.ExpiresAt || session.IsRevoked || session.Account == null)
            return Result<JWTResponse>.Failure(StatusCodes.Status401Unauthorized, "error.invalid_session");

        var generatedJwtResult = jwtService.GenerateNewJWT(session.Account);
        if (generatedJwtResult.IsError)
            return Result<JWTResponse>.Failure(generatedJwtResult.ErrorHttpCode, generatedJwtResult.ErrorIdentifier);

        var jwtTokens = generatedJwtResult.Value;
        if (jwtTokens == null)
            return Result<JWTResponse>.Failure(StatusCodes.Status500InternalServerError, "error.jwt_generated_null");

        session.RefreshToken = jwtTokens.RefreshToken;
        await db.SaveChangesAsync(cancellationToken);

        return Result<JWTResponse>.Success(jwtTokens);
    }

    private async Task<Result<JWTResponse>> SignupWithPassword(SignupRequest request, CancellationToken cancellationToken)
    {
        // [CHECK]: Password is null, empty or without valid characters?
        if (string.IsNullOrWhiteSpace(request.Password))
            return Result<JWTResponse>.Failure(StatusCodes.Status401Unauthorized, "error.invalid_password");

        // [CHECK]: Already exists a account with this name or email?
        var existsAccount = await db.Accounts.AnyAsync(acc => acc.Email == request.Email || acc.Name == request.FullName, cancellationToken);
        if (existsAccount)
            return Result<JWTResponse>.Failure(StatusCodes.Status401Unauthorized, "error.email_or_name_already_in_use");

        // Create a new account and add to database
        var createdAccount = new Account
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            Name = request.FullName
        };
        createdAccount.PasswordHash = passwordHasher.HashPassword(createdAccount, request.Password);

        await db.AddAsync(createdAccount, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        // Create a new access and refresh token to return back has cookie.
        var result = jwtService.GenerateNewJWT(createdAccount);
        if (result.IsError)
            return Result<JWTResponse>.Failure(result.ErrorHttpCode, "error.signup_access_failed");

        if (result.Value == null)
            return Result<JWTResponse>.Failure(result.ErrorHttpCode, "error.signup_access_null_value");

        // Create a session and save in database
        var createdSession = new AccountSession
        {
            Id = Guid.NewGuid(),
            AccountId = createdAccount.Id,

            Device = "Unknown",
            RefreshToken = result.Value.RefreshToken,

            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await db.AddAsync(createdSession, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return Result<JWTResponse>.Success(result.Value);
    }
    private async Task<Result<JWTResponse>> SignupWithOAuth(SignupRequest signupRequest, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
