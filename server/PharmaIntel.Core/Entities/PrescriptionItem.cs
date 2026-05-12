// =============================================================================
// Entity: PrescriptionItem (Thuoc trong don)
// Chuc nang: Chi tiet tung loai thuoc trong don thuoc (lieu luong, tan suat, thoi gian).
// Quan he: N:1 voi Prescription, Medication (nullable) | 1:N voi MedicationReminder.
// Luu y: medication_name la snapshot de giu lich su neu thuoc doi ten.
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class PrescriptionItem
{
    public long Id { get; set; }
    public long PrescriptionId { get; set; }
    public long? MedicationId { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public string? Duration { get; set; }

    // Single-use flag: true sau khi don thuoc duoc dung de checkout thanh cong.
    // Restore ve false khi don hang lien quan bi cancel truoc khi delivered.
    public bool IsDispensed { get; set; }

    public Prescription Prescription { get; set; } = null!;
    public Medication? Medication { get; set; }
    public ICollection<MedicationReminder> Reminders { get; set; } = new List<MedicationReminder>();
}
