// =============================================================================
// DTOs: Create/Update/Query cho MedicationReminder + Log.
// Validation: o Validators/MedicationReminders/*.
// =============================================================================
using PharmaIntel.Core.DTOs.Common;

namespace PharmaIntel.Core.DTOs.MedicationReminders;

public class MedicationReminderCreateRequest
{
    public long? PrescriptionItemId { get; set; }     // optional - link toi item don thuoc
    public string? MedicationName { get; set; }       // bat buoc neu khong co PrescriptionItemId
    public string FrequencyType { get; set; } = "daily"; // once, daily, weekly, custom
    public TimeOnly ReminderTime { get; set; }
    public DateOnly? StartDate { get; set; }          // null -> service set today
    public DateOnly? EndDate { get; set; }            // null = mo (chua biet ngay dung)
}

public class MedicationReminderUpdateRequest
{
    public long? PrescriptionItemId { get; set; }
    public string? MedicationName { get; set; }
    public string FrequencyType { get; set; } = "daily";
    public TimeOnly ReminderTime { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string Status { get; set; } = "active";    // active, paused, completed, cancelled
}

public class MedicationReminderListQuery : PagedQuery
{
    public string? Status { get; set; }                // filter theo status
    public string? Q { get; set; }                     // tim theo medicationName
}

public class MedicationReminderLogCreateRequest
{
    public DateTime ScheduledAt { get; set; }          // slot gio dang nhac
    public string Status { get; set; } = "taken";      // taken, missed, skipped (khong cho post "scheduled")
}

public class MedicationReminderLogListQuery : PagedQuery
{
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }            // loc theo ScheduledAt >= FromDate
    public DateTime? ToDate { get; set; }              // loc theo ScheduledAt <= ToDate
}
