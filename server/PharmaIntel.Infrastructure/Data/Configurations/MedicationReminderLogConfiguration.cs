// =============================================================================
// EF Core Configuration: MedicationReminderLog
// Chuc nang: Cau hinh bang medication_reminder_logs - check status, index.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Infrastructure.Data.Configurations;

public class MedicationReminderLogConfiguration : IEntityTypeConfiguration<MedicationReminderLog>
{
    public void Configure(EntityTypeBuilder<MedicationReminderLog> builder)
    {
        builder.ToTable("medication_reminder_logs");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ScheduledAt).HasColumnType("datetime2(0)").IsRequired();
        builder.Property(e => e.CompletedAt).HasColumnType("datetime2(0)");
        builder.Property(e => e.Status).HasMaxLength(20).IsRequired().HasDefaultValue("scheduled");

        builder.HasOne(e => e.Reminder)
            .WithMany(r => r.Logs)
            .HasForeignKey(e => e.ReminderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.ReminderId, e.ScheduledAt })
            .HasDatabaseName("IX_medication_reminder_logs_reminder_scheduled_at");

        builder.ToTable(t => t.HasCheckConstraint("CK_medication_reminder_logs_status",
            "[status] IN ('scheduled','taken','missed','skipped')"));
    }
}
