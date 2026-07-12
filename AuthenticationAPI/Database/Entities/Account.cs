using Microsoft.AspNetCore.Http.HttpResults;

namespace AuthenticationAPI.Database.Entities;

public class Account
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<AccountSession>? AccountSessions { get; set; }
}