// =============================================================================
// EF Core Configuration: DiagnosticResultMedication
// Chuc nang: Cau hinh bang trung gian diagnostic_result_medications.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Infrastructure.Data.Configurations;

public class DiagnosticResultMedicationConfiguration : IEntityTypeConfiguration<DiagnosticResultMedication>
{
    public void Configure(EntityTypeBuilder<DiagnosticResultMedication> builder)
    {
        builder.ToTable("diagnostic_result_medications");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Priority).IsRequired();

        builder.HasOne(e => e.Result)
            .WithMany(r => r.ResultMedications)
            .HasForeignKey(e => e.ResultId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Medication)
            .WithMany(m => m.DiagnosticResultMedications)
            .HasForeignKey(e => e.MedicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.ResultId, e.MedicationId })
            .IsUnique()
            .HasDatabaseName("UQ_diagnostic_result_medications");

        builder.ToTable(t => t.HasCheckConstraint("CK_diagnostic_result_medications_priority",
            "[priority] > 0"));
    }
}
