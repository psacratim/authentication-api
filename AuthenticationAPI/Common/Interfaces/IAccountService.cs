using AuthenticationAPI.Common.Results;
using AuthenticationAPI.Database.Entities;

namespace AuthenticationAPI.Common.Interfaces;

public interface IAccountService
{
    public Task<Result<Account>> CreateAccount(string email, string fullName, string? password, CancellationToken cancellationToken = default);
    public Task<Account?> GetAccountByEmail(string email, CancellationToken cancellationToken = default);
    public Task<Result> ValidatePassword(Account account, string? password, CancellationToken cancellationToken = default);
}
