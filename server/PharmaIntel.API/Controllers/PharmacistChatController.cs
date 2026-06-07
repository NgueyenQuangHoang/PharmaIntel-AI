// =============================================================================
// Controller: PharmacistChatController
// Chuc nang: Endpoint cho duoc si xem danh sach phien chat de tiep quan.
//   - GET /api/pharmacist/chat/sessions?status=waiting|open
// Xem tin nhan va gui tin: dung lai endpoint/hub chung (GET /api/chat/{id}/messages,
// SignalR ChatHub.SendMessage) vi quyen truy cap da xu ly o IChatService.
// =============================================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaIntel.API.Extensions;
using PharmaIntel.Core.DTOs.Chat;
using PharmaIntel.Core.Interfaces.Services;

namespace PharmaIntel.API.Controllers;

[ApiController]
[Authorize(Roles = "pharmacist")]
[Route("api/pharmacist/chat")]
public class PharmacistChatController : ControllerBase
{
    private readonly IChatService _service;

    public PharmacistChatController(IChatService service)
    {
        _service = service;
    }

    // Hang cho (waiting) + cac phien duoc si nay da nhan (open). status null = ca hai.
    [HttpGet("sessions")]
    public async Task<ActionResult<IReadOnlyList<ChatSessionListItemDto>>> ListSessions(
        [FromQuery] string? status, CancellationToken ct)
    {
        var sessions = await _service.GetSessionsForPharmacistAsync(User.GetUserId(), status, ct);
        return Ok(sessions);
    }
}
