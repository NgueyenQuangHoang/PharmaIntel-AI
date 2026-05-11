// =============================================================================
// EF Core Configuration: MedicationReminder
// Chuc nang: Cau hinh bang medication_reminders - check frequency_type, status.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Infrastructure.Data.Configurations;

public class MedicationReminderConfiguration : IEntityTypeConfiguration<MedicationReminder>
{
    public void Configure(EntityTypeBuilder<MedicationReminder> builder)
    {
        builder.ToTable("medication_reminders");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.MedicationName).HasMaxLength(255).IsRequired();
        builder.Property(e => e.FrequencyType).HasMaxLength(20).IsRequired().HasDefaultValue("daily");
        builder.Property(e => e.ReminderTime).HasColumnType("time(0)");
        builder.Property(e => e.StartDate).HasColumnType("date").IsRequired();
        builder.Property(e => e.EndDate).HasColumnType("date");
        builder.Property(e => e.Status).HasMaxLength(20).IsRequired().HasDefaultValue("active");
        builder.Property(e => e.CreatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(e => e.UpdatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(e => e.User)
            .WithMany(u => u.MedicationReminders)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.PrescriptionItem)
            .WithMany(pi => pi.Reminders)
            .HasForeignKey(e => e.PrescriptionItemId)
            .OnDelete(DeleteBehavior.NoAction);

        // Filtered unique: moi PrescriptionItem chi co toi da 1 reminder o moi gio
        // trong ngay -> chan duplicate khi verify chay song song. Khong ap dung cho
        // reminder standalone (PrescriptionItemId NULL) vi user co the tao bao nhieu tuy y.
        builder.HasIndex(e => new { e.PrescriptionItemId, e.ReminderTime })
            .IsUnique()
            .HasFilter("[prescription_item_id] IS NOT NULL")
            .HasDatabaseName("UX_medication_reminders_item_time");

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_medication_reminders_frequency",
                "[frequency_type] IN ('once','daily','weekly','custom')");
            t.HasCheckConstraint("CK_medication_reminders_status",
                "[status] IN ('active','paused','completed','cancelled')");
            t.HasCheckConstraint("CK_medication_reminders_date_range",
                "[end_date] IS NULL OR [end_date] >= [start_date]");
        });
    }
}
