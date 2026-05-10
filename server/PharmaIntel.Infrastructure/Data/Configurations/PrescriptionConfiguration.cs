// =============================================================================
// EF Core Configuration: Prescription
// Chuc nang: Cau hinh bang prescriptions - check status, verification_status.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Infrastructure.Data.Configurations;

public class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
{
    public void Configure(EntityTypeBuilder<Prescription> builder)
    {
        builder.ToTable("prescriptions");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.DoctorNameSnapshot).HasMaxLength(255);
        builder.Property(e => e.Title).HasMaxLength(255);
        builder.Property(e => e.PrescribedDate).HasColumnType("date");
        builder.Property(e => e.Status).HasMaxLength(20).IsRequired().HasDefaultValue("draft");
        builder.Property(e => e.VerificationStatus).HasMaxLength(20).IsRequired().HasDefaultValue("not_required");
        builder.Property(e => e.CreatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(e => e.UpdatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");

        // Restrict: don thuoc la ban ghi y te phap ly - phai giu kha ca khi user bi xoa.
        builder.HasOne(e => e.User)
            .WithMany(u => u.Prescriptions)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Doctor)
            .WithMany(d => d.Prescriptions)
            .HasForeignKey(e => e.DoctorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_prescriptions_status",
                "[status] IN ('draft','active','completed','expired','cancelled')");
            t.HasCheckConstraint("CK_prescriptions_verification",
                "[verification_status] IN ('not_required','pending','verified','rejected')");
        });
    }
}
