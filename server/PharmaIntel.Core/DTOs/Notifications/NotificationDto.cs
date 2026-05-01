// =============================================================================
// DTOs: NotificationDto, UnreadCountDto, MarkAllReadResultDto, NotificationListQuery
// Chuc nang: Tra ve thong bao cho user dang dang nhap.
// User KHONG tu tao notification - system tao luc co event (order, reminder, ...).
// =============================================================================
using PharmaIntel.Core.DTOs.Common;

namespace PharmaIntel.Core.DTOs.Notifications;

public class NotificationDto
{
    public long Id { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Body { get; set; }
    public string? ReferenceType { get; set; }
    public long? ReferenceId { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UnreadCountDto
{
    public int Count { get; set; }
}

public class MarkAllReadResultDto
{
    public int UpdatedCount { get; set; }
}

public class NotificationListQuery : PagedQuery
{
    public bool? IsRead { get; set; }                // filter da doc / chua doc
    public string? Type { get; set; }                // filter theo notificationType
}
