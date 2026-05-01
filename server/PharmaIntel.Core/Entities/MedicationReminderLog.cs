// =============================================================================
// Entity: MedicationReminderLog (Log tung lan nhac thuoc)
// Chuc nang: Ghi nhan tung lan nhac cu the (da uong / bo lo / bo qua).
// Quan he: N:1 voi MedicationReminder.
// Trang thai: scheduled -> taken / missed / skipped.
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class MedicationReminderLog
{
    public long Id { get; set; }
    public long ReminderId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = "scheduled"; // scheduled, taken, missed, skipped

    public MedicationReminder Reminder { get; set; } = null!;
}
