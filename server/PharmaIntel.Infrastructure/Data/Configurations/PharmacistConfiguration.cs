// =============================================================================
// EF Core Configuration: Pharmacist
// Chuc nang: Cau hinh bang pharmacists - unique license_number.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Infrastructure.Data.Configurations;

public class PharmacistConfiguration : IEntityTypeConfiguration<Pharmacist>
{
    public void Configure(EntityTypeBuilder<Pharmacist> builder)
    {
        builder.ToTable("pharmacists");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId);
        builder.Property(e => e.FullName).HasMaxLength(255).IsRequired();
        builder.Property(e => e.LicenseNumber).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Specialization).HasMaxLength(255);
        builder.Property(e => e.Phone).HasMaxLength(20);
        builder.Property(e => e.Email).HasMaxLength(255);
        builder.Property(e => e.AvatarUrl).HasMaxLength(500);
        builder.Property(e => e.IsOnline).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(e => e.CreatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(e => e.UpdatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(e => e.LicenseNumber).IsUnique().HasDatabaseName("UQ_pharmacists_license_number");

        // Moi user chi map toi da 1 pharmacist profile. Filtered de cho phep nhieu profile chua gan UserId.
        builder.HasIndex(e => e.UserId)
            .IsUnique()
            .HasFilter("[user_id] IS NOT NULL")
            .HasDatabaseName("UX_pharmacists_user_id");

        // Restrict: giu profile khi user soft-deleted - dong bo voi policy bao ve du lieu.
        builder.HasOne(e => e.User)
            .WithOne(u => u.PharmacistProfile)
            .HasForeignKey<Pharmacist>(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_pharmacists_users_user_id");
    }
}
