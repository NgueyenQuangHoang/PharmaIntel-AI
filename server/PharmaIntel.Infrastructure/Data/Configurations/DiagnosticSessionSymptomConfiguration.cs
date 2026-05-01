// =============================================================================
// EF Core Configuration: DiagnosticSessionSymptom
// Chuc nang: Cau hinh bang trung gian diagnostic_session_symptoms - unique composite.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Infrastructure.Data.Configurations;

public class DiagnosticSessionSymptomConfiguration : IEntityTypeConfiguration<DiagnosticSessionSymptom>
{
    public void Configure(EntityTypeBuilder<DiagnosticSessionSymptom> builder)
    {
        builder.ToTable("diagnostic_session_symptoms");
        builder.HasKey(e => e.Id);

        builder.HasOne(e => e.Session)
            .WithMany(s => s.SessionSymptoms)
            .HasForeignKey(e => e.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Symptom)
            .WithMany(s => s.SessionSymptoms)
            .HasForeignKey(e => e.SymptomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.SessionId, e.SymptomId })
            .IsUnique()
            .HasDatabaseName("UQ_diagnostic_session_symptoms");
    }
}
