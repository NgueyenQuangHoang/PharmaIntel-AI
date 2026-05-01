// =============================================================================
// Entity: AuditLog (Nhat ky thao tac)
// Chuc nang: Ghi nhan cac thao tac nhay cam (xac minh don, thay doi don hang, xem du lieu).
// Quan he: N:1 voi User (actor, nullable).
// Luu y: Khong cho phep sua/xoa qua API thong thuong.
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class AuditLog
{
    public long Id { get; set; }
    public long? ActorUserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public long? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? ActorUser { get; set; }
}
