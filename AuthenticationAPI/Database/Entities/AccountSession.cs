using Microsoft.AspNetCore.Http.HttpResults;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AuthenticationAPI.Database.Entities;

public class AccountSession
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }

    public Guid RefreshToken { get; set; }
    public bool IsRevoked { get; set; }
    public string Device { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public Account? Account { get; set; }
}