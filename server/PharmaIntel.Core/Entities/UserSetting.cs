// =============================================================================
// Entity: UserSetting (Cai dat nguoi dung)
// Chuc nang: Luu cai dat ca nhan cua user (giao dien, ngon ngu, thong bao, am thanh).
// Quan he: 1:1 voi User (moi user co duy nhat 1 ban ghi setting).
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class UserSetting
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string DarkMode { get; set; } = "system"; // light, dark, system
    public string LanguageCode { get; set; } = "vi";
    public bool NotificationEnabled { get; set; } = true;
    public bool ReminderSoundEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
