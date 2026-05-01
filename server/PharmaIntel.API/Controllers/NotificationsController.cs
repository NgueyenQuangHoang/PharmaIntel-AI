// =============================================================================
// Controller: NotificationsController
// Chuc nang: User xem/danh dau da doc/xoa thong bao cua chinh minh.
// Endpoints (tat ca yeu cau JWT):
//   GET    /api/notifications                     list (paged + filter isRead/type)
//   GET    /api/notifications/unread-count        dem so noti chua doc
//   PUT    /api/notifications/{id}/read           danh dau 1 noti da doc (idempotent)
//   PUT    /api/notifications/read-all            danh dau tat ca da doc
//   DELETE /api/notifications/{id}                xoa 1 noti
// Khong co POST: notification do system tao thong qua event noi bo.
// =============================================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaIntel.API.Extensions;
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.DTOs.Notifications;
using PharmaIntel.Core.Interfaces.Services;

namespace PharmaIntel.API.Controllers;

[ApiController]
[Authorize]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _service;

    public NotificationsController(INotificationService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<NotificationDto>>> List(
        [FromQuery] NotificationListQuery query, CancellationToken ct)
        => Ok(await _service.ListMyAsync(User.GetUserId(), query, ct));

    [HttpGet("unread-count")]
    public async Task<ActionResult<UnreadCountDto>> UnreadCount(CancellationToken ct)
        => Ok(await _service.GetUnreadCountAsync(User.GetUserId(), ct));

    [HttpPut("{id:long}/read")]
    public async Task<ActionResult<NotificationDto>> MarkRead(long id, CancellationToken ct)
        => Ok(await _service.MarkReadAsync(User.GetUserId(), id, ct));

    [HttpPut("read-all")]
    public async Task<ActionResult<MarkAllReadResultDto>> MarkAllRead(CancellationToken ct)
        => Ok(await _service.MarkAllReadAsync(User.GetUserId(), ct));

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await _service.DeleteAsync(User.GetUserId(), id, ct);
        return NoContent();
    }
}
