// =============================================================================
// EF Core Configuration: Notification
// Chuc nang: Cau hinh bang notifications - index cho danh sach chua doc.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Infrastructure.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.NotificationType).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Title).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Body).HasColumnType("nvarchar(max)");
        builder.Property(e => e.ReferenceType).HasMaxLength(50);
        builder.Property(e => e.IsRead).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.ReadAt).HasColumnType("datetime2(0)");
        builder.Property(e => e.CreatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(e => e.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.UserId, e.IsRead })
            .HasDatabaseName("IX_notifications_user_read");
    }
}
