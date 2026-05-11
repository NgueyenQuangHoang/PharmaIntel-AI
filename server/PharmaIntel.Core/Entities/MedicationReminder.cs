// =============================================================================
// Entity: MedicationReminder (Lich nhac thuoc)
// Chuc nang: Cau hinh lich nhac uong thuoc (tan suat, gio nhac, trang thai).
// Quan he: N:1 voi User, PrescriptionItem (nullable) | 1:N voi MedicationReminderLog.
// Luu y: Chi mo ta lich nhac, trang thai tung lan nhac nam o MedicationReminderLog.
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class MedicationReminder
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long? PrescriptionItemId { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string FrequencyType { get; set; } = "daily"; // once, daily, weekly, custom
    public TimeOnly ReminderTime { get; set; }
    // Pham vi hieu luc: StartDate = ngay bat dau (NOT NULL). EndDate null = mo (thuoc man tinh
    // / chua biet ngay dung). Khi list reminders, service tu set Status='completed' neu
    // EndDate < today - tranh hien reminder qua han ma user khong phai thao tac.
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string Status { get; set; } = "active"; // active, paused, completed, cancelled
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public PrescriptionItem? PrescriptionItem { get; set; }
    public ICollection<MedicationReminderLog> Logs { get; set; } = new List<MedicationReminderLog>();
}
