// =============================================================================
// Entity: Prescription (Don thuoc)
// Chuc nang: Luu ho so don thuoc cua nguoi dung (bac si ke, trang thai xac minh).
// Quan he: N:1 voi User, Doctor (nullable) | 1:N voi PrescriptionItem,
//          PrescriptionDocument, Order.
// Luu y: doctor_id nullable vi co the la bac si ngoai he thong.
//        doctor_name_snapshot luu ten bac si tai thoi diem tao don.
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class Prescription
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long? DoctorId { get; set; }
    public string? DoctorNameSnapshot { get; set; }
    public string? Title { get; set; }
    public DateOnly? PrescribedDate { get; set; }
    public string Status { get; set; } = "draft"; // draft, active, completed, expired, cancelled
    public string VerificationStatus { get; set; } = "not_required"; // not_required, pending, verified, rejected
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Doctor? Doctor { get; set; }
    public ICollection<PrescriptionItem> Items { get; set; } = new List<PrescriptionItem>();
    public ICollection<PrescriptionDocument> Documents { get; set; } = new List<PrescriptionDocument>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
