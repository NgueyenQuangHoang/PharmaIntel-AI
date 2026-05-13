// =============================================================================
// EF Config: KnowledgeChunk
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Infrastructure.Data.Configurations;

public class KnowledgeChunkConfiguration : IEntityTypeConfiguration<KnowledgeChunk>
{
    public void Configure(EntityTypeBuilder<KnowledgeChunk> builder)
    {
        builder.ToTable("knowledge_chunks");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Content)
            .IsRequired();

        builder.Property(x => x.VectorId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.MetadataJson)
            .HasColumnType("nvarchar(max)")
            .HasDefaultValue("{}");

        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(x => x.VectorId)
            .IsUnique()
            .HasDatabaseName("UQ_knowledge_chunks_vector_id");

        builder.HasIndex(x => new { x.DocumentId, x.ChunkIndex })
            .IsUnique()
            .HasDatabaseName("UQ_knowledge_chunks_document_chunk");

        builder.HasOne(x => x.Document)
            .WithMany(x => x.Chunks)
            .HasForeignKey(x => x.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
