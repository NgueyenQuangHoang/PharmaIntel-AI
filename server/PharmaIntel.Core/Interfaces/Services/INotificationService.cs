// =============================================================================
// Interface: INotificationService
// Chuc nang: Quan ly thong bao cua user (read-only + mark read + delete).
// =============================================================================
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.DTOs.Notifications;

namespace PharmaIntel.Core.Interfaces.Services;

public interface INotificationService
{
    Task<PagedResult<NotificationDto>> ListMyAsync(long userId, NotificationListQuery query, CancellationToken ct = default);
    Task<UnreadCountDto> GetUnreadCountAsync(long userId, CancellationToken ct = default);
    Task<NotificationDto> MarkReadAsync(long userId, long id, CancellationToken ct = default);
    Task<MarkAllReadResultDto> MarkAllReadAsync(long userId, CancellationToken ct = default);
    Task DeleteAsync(long userId, long id, CancellationToken ct = default);
}
