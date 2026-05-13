// =============================================================================
// EF Config: EmbeddingCache
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Infrastructure.Data.Configurations;

public class EmbeddingCacheConfiguration : IEntityTypeConfiguration<EmbeddingCache>
{
    public void Configure(EntityTypeBuilder<EmbeddingCache> builder)
    {
        builder.ToTable("embedding_cache");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TextHash)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.Model)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.VectorJson)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(x => new { x.TextHash, x.Model })
            .IsUnique()
            .HasDatabaseName("UQ_embedding_cache_hash_model");
    }
}
