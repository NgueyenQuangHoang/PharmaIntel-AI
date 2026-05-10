// =============================================================================
// Entity: PharmacistChatMessage (Tin nhan chat duoc si)
// Chuc nang: Luu tung tin nhan trong phien chat voi duoc si.
// Quan he: N:1 voi PharmacistChatSession.
// sender_type: user | pharmacist | system.
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class PharmacistChatMessage
{
    public long Id { get; set; }
    public long SessionId { get; set; }
    public string SenderType { get; set; } = string.Empty; // user, pharmacist, system
    public long? SenderUserId { get; set; }
    public long? SenderPharmacistId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public PharmacistChatSession Session { get; set; } = null!;

    // Nav: chi co gia tri khi sender_type tuong ung. Check constraint dam bao tinh nhat quan.
    public User? SenderUser { get; set; }
    public Pharmacist? SenderPharmacist { get; set; }
}
