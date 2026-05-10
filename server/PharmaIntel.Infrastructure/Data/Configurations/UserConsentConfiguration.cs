// =============================================================================
// EF Core Configuration: UserConsent
// Chuc nang: Cau hinh bang user_consents - check consent_type.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Infrastructure.Data.Configurations;

public class UserConsentConfiguration : IEntityTypeConfiguration<UserConsent>
{
    public void Configure(EntityTypeBuilder<UserConsent> builder)
    {
        builder.ToTable("user_consents");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ConsentType).HasMaxLength(50).IsRequired();
        builder.Property(e => e.ConsentVersion).HasMaxLength(50).IsRequired();
        builder.Property(e => e.AcceptedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(e => e.RevokedAt).HasColumnType("datetime2(0)");
        builder.Property(e => e.IpAddress).HasMaxLength(45);
        builder.Property(e => e.UserAgent).HasMaxLength(500);

        // Restrict: bang chung phap ly user da dong y dieu khoan - khong duoc xoa.
        builder.HasOne(e => e.User)
            .WithMany(u => u.Consents)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(t => t.HasCheckConstraint("CK_user_consents_type",
            "[consent_type] IN ('terms','privacy','medical_ai_disclaimer','marketing')"));
    }
}
