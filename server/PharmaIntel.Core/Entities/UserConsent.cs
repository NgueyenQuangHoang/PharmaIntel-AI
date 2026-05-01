// =============================================================================
// Entity: UserConsent (Dong y dieu khoan)
// Chuc nang: Luu lich su dong y dieu khoan, quyen rieng tu, canh bao AI y te.
// Quan he: N:1 voi User.
// Luu y: Khong xoa - giu lai de audit va tuan thu phap luat.
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class UserConsent
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string ConsentType { get; set; } = string.Empty; // terms, privacy, medical_ai_disclaimer, marketing
    public string ConsentVersion { get; set; } = string.Empty;
    public DateTime AcceptedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public User User { get; set; } = null!;
}
