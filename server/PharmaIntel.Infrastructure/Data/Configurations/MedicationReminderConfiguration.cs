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

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_medication_reminders_frequency",
                "[frequency_type] IN ('once','daily','weekly','custom')");
            t.HasCheckConstraint("CK_medication_reminders_status",
                "[status] IN ('active','paused','completed','cancelled')");
        });
    }
}
