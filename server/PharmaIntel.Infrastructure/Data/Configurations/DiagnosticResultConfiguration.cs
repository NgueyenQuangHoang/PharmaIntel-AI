// =============================================================================
// EF Core Configuration: DiagnosticResult
// Chuc nang: Cau hinh bang diagnostic_results - 1:1 voi session, check risk/confidence.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Infrastructure.Data.Configurations;

public class DiagnosticResultConfiguration : IEntityTypeConfiguration<DiagnosticResult>
{
    public void Configure(EntityTypeBuilder<DiagnosticResult> builder)
    {
        builder.ToTable("diagnostic_results");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.AiConclusion).HasColumnType("nvarchar(max)");
        builder.Property(e => e.ConfidenceScore).HasColumnType("decimal(5,2)").IsRequired();
        builder.Property(e => e.RiskLevel).HasMaxLength(20).IsRequired().HasDefaultValue("low");
        builder.Property(e => e.RedFlags).HasColumnType("nvarchar(max)");
        builder.Property(e => e.RequiresDoctorVisit).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.ModelName).HasMaxLength(100);
        builder.Property(e => e.ModelVersion).HasMaxLength(50);
        builder.Property(e => e.DiagnosedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(e => e.SessionId).IsUnique().HasDatabaseName("UQ_diagnostic_results_session_id");

        builder.HasOne(e => e.Session)
            .WithOne(s => s.Result)
            .HasForeignKey<DiagnosticResult>(e => e.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.User)
            .WithMany(u => u.DiagnosticResults)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(e => new { e.UserId, e.DiagnosedAt })
            .HasDatabaseName("IX_diagnostic_results_user_diagnosed_at");

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_diagnostic_results_confidence", "[confidence_score] >= 0 AND [confidence_score] <= 100");
            t.HasCheckConstraint("CK_diagnostic_results_risk", "[risk_level] IN ('low','medium','high','emergency')");
        });
    }
}
