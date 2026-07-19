using AuthenticationAPI.Common.Results;
using AuthenticationAPI.Common.Types;
using AuthenticationAPI.Database.Entities;

namespace AuthenticationAPI.Common.Interfaces;

public interface ISessionService
{
    Task<Result<UserSession>> CreateSession(Account account, CancellationToken cancellationToken = default);
    Task<AccountSession?> GetSessionByRefreshToken(Guid refreshToken, CancellationToken cancellationToken = default);
    Task<Result<UserSession>> RefreshSession(AccountSession? session, CancellationToken cancellationToken = default);
    Task<Result> RevokeSession(AccountSession session, CancellationToken cancellationToken = default);
}
