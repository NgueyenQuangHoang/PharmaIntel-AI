// =============================================================================
// EF Core Configuration: PrescriptionDocument
// Chuc nang: Cau hinh bang prescription_documents - check verification_status.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Infrastructure.Data.Configurations;

public class PrescriptionDocumentConfiguration : IEntityTypeConfiguration<PrescriptionDocument>
{
    public void Configure(EntityTypeBuilder<PrescriptionDocument> builder)
    {
        builder.ToTable("prescription_documents");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.FileUrl).HasMaxLength(500).IsRequired();
        builder.Property(e => e.VerificationStatus).HasMaxLength(20).IsRequired().HasDefaultValue("pending");
        builder.Property(e => e.Notes).HasColumnType("nvarchar(max)");
        builder.Property(e => e.CreatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(e => e.UpdatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(e => e.Prescription)
            .WithMany(p => p.Documents)
            .HasForeignKey(e => e.PrescriptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.VerifiedByPharmacist)
            .WithMany(p => p.VerifiedDocuments)
            .HasForeignKey(e => e.VerifiedByPharmacistId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.ToTable(t => t.HasCheckConstraint("CK_prescription_documents_verification",
            "[verification_status] IN ('pending','verified','rejected')"));
    }
}
