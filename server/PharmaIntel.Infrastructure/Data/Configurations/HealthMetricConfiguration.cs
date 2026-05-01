// =============================================================================
// EF Core Configuration: HealthMetric
// Chuc nang: Cau hinh bang health_metrics - check metric_type, index cho bieu do.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Infrastructure.Data.Configurations;

public class HealthMetricConfiguration : IEntityTypeConfiguration<HealthMetric>
{
    public void Configure(EntityTypeBuilder<HealthMetric> builder)
    {
        builder.ToTable("health_metrics");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.MetricType).HasMaxLength(30).IsRequired();
        builder.Property(e => e.ValueNumber).HasColumnType("decimal(10,2)").IsRequired();
        builder.Property(e => e.ValueNumber2).HasColumnType("decimal(10,2)");
        builder.Property(e => e.Unit).HasMaxLength(20);
        builder.Property(e => e.Notes).HasColumnType("nvarchar(max)");
        builder.Property(e => e.RecordedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(e => e.User)
            .WithMany(u => u.HealthMetrics)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.UserId, e.MetricType, e.RecordedAt })
            .HasDatabaseName("IX_health_metrics_user_type_recorded_at");

        builder.ToTable(t => t.HasCheckConstraint("CK_health_metrics_type",
            "[metric_type] IN ('blood_pressure','heart_rate','temperature','weight','blood_sugar','oxygen_saturation')"));
    }
}
