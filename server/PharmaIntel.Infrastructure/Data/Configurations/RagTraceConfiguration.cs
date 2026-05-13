// =============================================================================
// EF Config: RagTrace
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Infrastructure.Data.Configurations;

public class RagTraceConfiguration : IEntityTypeConfiguration<RagTrace>
{
    public void Configure(EntityTypeBuilder<RagTrace> builder)
    {
        builder.ToTable("rag_traces");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserMessage)
            .IsRequired();

        builder.Property(x => x.MedicationContextJson)
            .HasColumnType("nvarchar(max)")
            .HasDefaultValue("[]");

        builder.Property(x => x.KnowledgeContextJson)
            .HasColumnType("nvarchar(max)")
            .HasDefaultValue("[]");

        builder.Property(x => x.AiResponse)
            .IsRequired();

        // Phase 5: latency + error tracking
        builder.Property(x => x.RetrievalLatencyMs).HasDefaultValue(0);
        builder.Property(x => x.GenerationLatencyMs).HasDefaultValue(0);
        builder.Property(x => x.TotalLatencyMs).HasDefaultValue(0);
        builder.Property(x => x.ErrorType).HasMaxLength(100);

        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(x => x.DiagnosticSessionId)
            .HasDatabaseName("IX_rag_traces_session_id");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("IX_rag_traces_created_at");

        builder.HasOne(x => x.DiagnosticSession)
            .WithMany()
            .HasForeignKey(x => x.DiagnosticSessionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
