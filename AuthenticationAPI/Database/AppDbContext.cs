using AuthenticationAPI.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationAPI.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<AccountSession> AccountSessions => Set<AccountSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
