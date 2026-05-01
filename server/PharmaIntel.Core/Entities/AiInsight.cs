// =============================================================================
// Entity: AiInsight (Insight AI ca nhan hoa)
// Chuc nang: Luu cac goi y/phan tich AI danh rieng cho nguoi dung.
// Quan he: N:1 voi User.
// Loai: health_summary, medication, diagnostic, lifestyle, system.
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class AiInsight
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string InsightType { get; set; } = string.Empty; // health_summary, medication, diagnostic, lifestyle, system
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
