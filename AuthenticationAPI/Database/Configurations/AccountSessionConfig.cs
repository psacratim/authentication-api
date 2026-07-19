using AuthenticationAPI.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthenticationAPI.Database.Configurations;

public class AccountSessionConfig : IEntityTypeConfiguration<AccountSession>
{
    public void Configure(EntityTypeBuilder<AccountSession> builder)
    {
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).ValueGeneratedNever();

        builder.Property(entity => entity.RefreshToken).IsRequired();
        builder.Property(entity => entity.IsRevoked).IsRequired().HasDefaultValue(false);
        builder.Property(entity => entity.Device).IsRequired().HasDefaultValue("Unknown");
        builder.Property(entity => entity.CreatedAt).IsRequired().HasDefaultValueSql("NOW()");
        builder.Property(entity => entity.ExpiresAt).IsRequired();

        builder.HasIndex(entity => entity.RefreshToken).IsUnique();

        builder
            .HasOne(session => session.Account)
            .WithMany(account => account.AccountSessions)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_AccountSessions_Account");
    }
}