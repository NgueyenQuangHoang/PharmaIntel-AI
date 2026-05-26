// =============================================================================
// Entity: Consultation (Lich tu van duoc si)
// Chuc nang: Luu yeu cau dat lich tu van giua nguoi dung va duoc si.
// Quan he: N:1 voi User (nguoi dat) | N:1 voi Pharmacist (duoc si phu trach).
// Trang thai: pending -> accepted -> completed | rejected | cancelled.
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class Consultation
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long PharmacistId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public string? Note { get; set; }
    public string Status { get; set; } = "pending"; // pending, accepted, rejected, completed, cancelled
    public string? ResponseNote { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Pharmacist Pharmacist { get; set; } = null!;
}
