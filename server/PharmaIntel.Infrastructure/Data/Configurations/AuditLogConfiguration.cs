// =============================================================================
// EF Core Configuration: AuditLog
// Chuc nang: Cau hinh bang audit_logs - khong cho phep sua/xoa qua API.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Action).HasMaxLength(100).IsRequired();
        builder.Property(e => e.EntityName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.OldValues).HasColumnType("nvarchar(max)");
        builder.Property(e => e.NewValues).HasColumnType("nvarchar(max)");
        builder.Property(e => e.CreatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(e => e.ActorUser)
            .WithMany(u => u.AuditLogs)
            .HasForeignKey(e => e.ActorUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
