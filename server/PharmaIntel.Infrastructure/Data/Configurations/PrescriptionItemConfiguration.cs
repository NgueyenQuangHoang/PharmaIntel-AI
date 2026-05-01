// =============================================================================
// EF Core Configuration: PrescriptionItem
// Chuc nang: Cau hinh bang prescription_items.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Infrastructure.Data.Configurations;

public class PrescriptionItemConfiguration : IEntityTypeConfiguration<PrescriptionItem>
{
    public void Configure(EntityTypeBuilder<PrescriptionItem> builder)
    {
        builder.ToTable("prescription_items");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.MedicationName).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Dosage).HasMaxLength(100);
        builder.Property(e => e.Frequency).HasMaxLength(100);
        builder.Property(e => e.Duration).HasMaxLength(100);

        builder.HasOne(e => e.Prescription)
            .WithMany(p => p.Items)
            .HasForeignKey(e => e.PrescriptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Medication)
            .WithMany(m => m.PrescriptionItems)
            .HasForeignKey(e => e.MedicationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
