// =============================================================================
// EF Core Configuration: AiInsight
// Chuc nang: Cau hinh bang ai_insights - check insight_type.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Infrastructure.Data.Configurations;

public class AiInsightConfiguration : IEntityTypeConfiguration<AiInsight>
{
    public void Configure(EntityTypeBuilder<AiInsight> builder)
    {
        builder.ToTable("ai_insights");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.InsightType).HasMaxLength(30).IsRequired();
        builder.Property(e => e.Title).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Content).HasColumnType("nvarchar(max)");
        builder.Property(e => e.GeneratedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(e => e.User)
            .WithMany(u => u.AiInsights)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(t => t.HasCheckConstraint("CK_ai_insights_type",
            "[insight_type] IN ('health_summary','medication','diagnostic','lifestyle','system')"));
    }
}
