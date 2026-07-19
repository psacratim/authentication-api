using AuthenticationAPI.Common.Interfaces;
using AuthenticationAPI.Common.Results;
using AuthenticationAPI.Common.Types;
using AuthenticationAPI.Database;
using AuthenticationAPI.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationAPI.Services;

public class SessionService(
    IJWTService jwtService,
    AppDbContext db,
    IHttpContextAccessor httpContextAccessor
) : ISessionService
{
    public async Task<Result<UserSession>> CreateSession(Account account, CancellationToken cancellationToken = default)
    {
        var deviceName = GetUserDevice();
        var result = jwtService.GenerateNewJWT(account, deviceName);
        if (result.IsError)
            return Result<UserSession>.Failure("error.jwt_service_internal");

        var tokens = result.Value;
        if (tokens == null)
            return Result<UserSession>.Failure("error.jwt_service_value_null");

        var createdSession = new AccountSession
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,

            Device = deviceName,
            RefreshToken = tokens.RefreshToken,

            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await db.AddAsync(createdSession, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return Result<UserSession>.Success(new UserSession(
            AccessToken: tokens.AccessToken,
            RefreshToken: tokens.RefreshToken,
            DeviceName: deviceName
        ));
    }

    public async Task<AccountSession?> GetSessionByRefreshToken(Guid refreshToken, CancellationToken cancellationToken = default)
    {
        var session = await db.AccountSessions.Include(sess => sess.Account).FirstOrDefaultAsync(sess => sess.RefreshToken == refreshToken, cancellationToken);

        return session;
    }

    public async Task<Result> RevokeSession(AccountSession session, CancellationToken cancellationToken = default)
    {
        session.IsRevoked = true;
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<UserSession>> RefreshSession(AccountSession? session, CancellationToken cancellationToken = default)
    {
        if (session == null || DateTime.UtcNow > session.ExpiresAt || session.IsRevoked || session.Account == null)
            return Result<UserSession>.Failure(StatusCodes.Status401Unauthorized, "error.invalid_session");

        var result = jwtService.GenerateNewJWT(session.Account, session.Device);
        if (result.IsError)
            return Result<UserSession>.Failure("error.jwt_service_internal");

        var tokens = result.Value;
        if (tokens == null)
            return Result<UserSession>.Failure("error.jwt_service_value_null");

        session.RefreshToken = tokens.RefreshToken;
        await db.SaveChangesAsync(cancellationToken);

        return Result<UserSession>.Success(new UserSession(
            AccessToken: tokens.AccessToken,
            RefreshToken: tokens.RefreshToken,
            DeviceName: session.Device
        ));
    }

    private string GetUserDevice()
    {
        var userAgent = httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();
        var deviceType = "Desktop";

        if (!string.IsNullOrEmpty(userAgent))
        {
            if (userAgent.Contains("Mobi", StringComparison.OrdinalIgnoreCase))
            {
                deviceType = "Mobile";
            }
            else if (userAgent.Contains("iPad", StringComparison.OrdinalIgnoreCase) ||
            userAgent.Contains("Tablet", StringComparison.OrdinalIgnoreCase))
                deviceType = "Tablet";
        }

        return deviceType;
    }
}
