// =============================================================================
// Entity: PharmacistChatSession (Phien chat voi duoc si)
// Chuc nang: Quan ly phien tro chuyen giua nguoi dung va duoc si.
// Quan he: N:1 voi User, Pharmacist (nullable) | 1:N voi PharmacistChatMessage.
// Trang thai: open -> waiting -> closed / cancelled.
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class PharmacistChatSession
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long? PharmacistId { get; set; }
    public string Status { get; set; } = "open"; // open, waiting, closed, cancelled
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }

    public User User { get; set; } = null!;
    public Pharmacist? Pharmacist { get; set; }
    public ICollection<PharmacistChatMessage> Messages { get; set; } = new List<PharmacistChatMessage>();
}
