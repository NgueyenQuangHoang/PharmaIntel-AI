// =============================================================================
// DTOs: MedicationReminderListItemDto, MedicationReminderDto, MedicationReminderLogDto
// Chuc nang: Tra ve thong tin lich nhac thuoc + log lan nhac.
// =============================================================================
namespace PharmaIntel.Core.DTOs.MedicationReminders;

public class MedicationReminderListItemDto
{
    public long Id { get; set; }
    public long? PrescriptionItemId { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string FrequencyType { get; set; } = "daily";
    public TimeOnly ReminderTime { get; set; }
    public string Status { get; set; } = "active";
    public int LogCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class MedicationReminderDto : MedicationReminderListItemDto
{
    // Detail co the mo rong them o tuong lai (vi du recent logs).
}

public class MedicationReminderLogDto
{
    public long Id { get; set; }
    public long ReminderId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = "scheduled";
}
