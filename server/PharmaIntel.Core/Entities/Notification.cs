// =============================================================================
// Entity: Notification (Thong bao)
// Chuc nang: Luu thong bao hien thi qua icon chuong/header cho nguoi dung.
// Quan he: N:1 voi User.
// Luu y: reference_type + reference_id cho phep lien ket polymorphic (order, prescription...).
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class Notification
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Body { get; set; }
    public string? ReferenceType { get; set; }
    public long? ReferenceId { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
