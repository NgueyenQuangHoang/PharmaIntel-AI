// =============================================================================
// EF Core Configuration: DiagnosticSession
// Chuc nang: Cau hinh bang diagnostic_sessions - check status.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Infrastructure.Data.Configurations;

public class DiagnosticSessionConfiguration : IEntityTypeConfiguration<DiagnosticSession>
{
    public void Configure(EntityTypeBuilder<DiagnosticSession> builder)
    {
        builder.ToTable("diagnostic_sessions");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status).HasMaxLength(30).IsRequired().HasDefaultValue("in_progress");
        builder.Property(e => e.CreatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(e => e.CompletedAt).HasColumnType("datetime2(0)");

        // Restrict: lich su chan doan AI thuoc ho so y te - giu kha ca khi user bi xoa.
        builder.HasOne(e => e.User)
            .WithMany(u => u.DiagnosticSessions)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(t => t.HasCheckConstraint("CK_diagnostic_sessions_status",
            "[status] IN ('in_progress','analyzing','completed','cancelled','failed')"));
    }
}
