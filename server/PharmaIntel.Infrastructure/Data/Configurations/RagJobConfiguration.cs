// =============================================================================
// EF Config: RagJob
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Infrastructure.Data.Configurations;

public class RagJobConfiguration : IEntityTypeConfiguration<RagJob>
{
    public void Configure(EntityTypeBuilder<RagJob> builder)
    {
        builder.ToTable("rag_jobs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.JobType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(30).IsRequired().HasDefaultValue("queued");
        builder.Property(x => x.PayloadJson).HasColumnType("nvarchar(max)").HasDefaultValue("{}");
        builder.Property(x => x.ErrorMessage).HasMaxLength(4000);

        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(x => x.StartedAt).HasColumnType("datetime2(0)");
        builder.Property(x => x.CompletedAt).HasColumnType("datetime2(0)");

        builder.HasIndex(x => new { x.Status, x.CreatedAt })
            .HasDatabaseName("IX_rag_jobs_status_created_at");

        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_rag_jobs_status",
                "[status] IN ('queued','running','completed','failed')");

            t.HasCheckConstraint(
                "CK_rag_jobs_type",
                "[job_type] IN ('ingest','reindex','delete_vector')");
        });
    }
}
