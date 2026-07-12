using AuthenticationAPI.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthenticationAPI.Database.Configurations;

public class AccountConfig : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).ValueGeneratedNever();

        builder.Property(entity => entity.Email).IsRequired().HasMaxLength(80);
        builder.Property(entity => entity.Name).IsRequired().HasMaxLength(128);

        builder.Property(entity => entity.CreatedAt).IsRequired().HasDefaultValueSql("NOW()");
    }
}