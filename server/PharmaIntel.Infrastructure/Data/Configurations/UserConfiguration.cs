// =============================================================================
// EF Core Configuration: User
// Chuc nang: Cau hinh bang users - kieu du lieu, unique, check constraints, index.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.FullName).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Email).HasMaxLength(255).IsRequired();
        builder.Property(e => e.PasswordHash).HasMaxLength(255);
        builder.Property(e => e.AvatarUrl).HasMaxLength(500);
        builder.Property(e => e.PhoneNumber).HasMaxLength(20);
        builder.Property(e => e.DateOfBirth).HasColumnType("date");
        builder.Property(e => e.AuthProvider).HasMaxLength(20).IsRequired().HasDefaultValue("local");
        builder.Property(e => e.AuthProviderId).HasMaxLength(255);
        builder.Property(e => e.Role).HasMaxLength(20).IsRequired().HasDefaultValue("user");
        builder.Property(e => e.IsTermsAccepted).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(e => e.CreatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(e => e.UpdatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(e => e.Email).IsUnique().HasDatabaseName("UQ_users_email");
        builder.HasIndex(e => e.AuthProviderId)
            .IsUnique()
            .HasFilter("[auth_provider_id] IS NOT NULL")
            .HasDatabaseName("UX_users_auth_provider_id");

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_users_auth_provider",
                "[auth_provider] IN ('local','google','apple')");
            t.HasCheckConstraint("CK_users_role",
                "[role] IN ('user','admin','pharmacist')");
        });
    }
}
