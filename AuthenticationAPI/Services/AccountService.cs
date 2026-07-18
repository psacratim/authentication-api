using AuthenticationAPI.Common.Interfaces;
using AuthenticationAPI.Common.Results;
using AuthenticationAPI.Database;
using AuthenticationAPI.Database.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationAPI.Services;

public class AccountService(
    AppDbContext db,
    IPasswordHasher<Account> passwordHasher
) : IAccountService
{
    public async Task<Result<Account>> CreateAccount(string email, string fullName, string? password, CancellationToken cancellationToken = default)
    {
        // [CHECK]: Password is null, empty or without valid characters?
        if (string.IsNullOrWhiteSpace(password))
            return Result<Account>.Failure(StatusCodes.Status401Unauthorized, "error.invalid_password");

        // [CHECK]: Already exists a account with this name or email?
        var existsAccount = await db.Accounts.AnyAsync(acc => acc.Email == email || acc.Name == fullName, cancellationToken);
        if (existsAccount)
            return Result<Account>.Failure(StatusCodes.Status401Unauthorized, "error.email_or_name_already_in_use");

        // Create a new account and add to database
        var createdAccount = new Account
        {
            Id = Guid.NewGuid(),
            Email = email,
            Name = fullName
        };
        createdAccount.PasswordHash = passwordHasher.HashPassword(createdAccount, password);

        await db.AddAsync(createdAccount, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return Result<Account>.Success(createdAccount);
    }

    public async Task<Account?> GetAccountByEmail(string email, CancellationToken cancellationToken = default)
    {
        var account = await db.Accounts.FirstOrDefaultAsync(acc => acc.Email == email, cancellationToken);

        return account;
    }

    public async Task<Result> ValidatePassword(Account account, string? password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(password))
            return Result.Failure(StatusCodes.Status401Unauthorized, "error.invalid_credentials");

        if (account.PasswordHash == null)
            return Result.Failure(StatusCodes.Status401Unauthorized, "error.using_password_on_oauth");

        if (passwordHasher.VerifyHashedPassword(account, account.PasswordHash, password) == PasswordVerificationResult.Failed)
        {
            return Result.Failure(StatusCodes.Status401Unauthorized, "error.invalid_credentials");
        }

        return Result.Success();
    }
}
