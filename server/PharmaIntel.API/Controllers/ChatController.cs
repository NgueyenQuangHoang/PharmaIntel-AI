// =============================================================================
// Controller: ChatController
// Chuc nang: REST endpoint phu tro cho chat real-time.
//   - POST /api/chat/session      : benh nhan tao/lay phien dang mo.
//   - GET  /api/chat/{id}/messages: lay lich su tin nhan cua phien.
// Phan real-time (gui/nhan tuc thoi) di qua SignalR ChatHub, khong qua day.
// Dung rate limit policy "ai-chat" san co de chong spam tao phien.
// =============================================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PharmaIntel.API.Extensions;
using PharmaIntel.Core.DTOs.Chat;
using PharmaIntel.Core.Interfaces.Services;

namespace PharmaIntel.API.Controllers;

[ApiController]
[Authorize]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly IChatService _service;

    public ChatController(IChatService service)
    {
        _service = service;
    }

    // Benh nhan mo man chat -> lay phien dang mo cua minh hoac tao moi (waiting).
    [HttpPost("session")]
    [EnableRateLimiting("ai-chat")]
    public async Task<ActionResult<ChatSessionDto>> GetOrCreateSession(CancellationToken ct)
    {
        var dto = await _service.GetOrCreateSessionForUserAsync(User.GetUserId(), ct);
        return Ok(dto);
    }

    // Lay lich su tin nhan (cuon len xem lai). Real-time moi tin di qua SignalR.
    [HttpGet("{sessionId:long}/messages")]
    public async Task<ActionResult<IReadOnlyList<ChatMessageDto>>> GetMessages(
        long sessionId, CancellationToken ct)
    {
        var messages = await _service.GetMessagesAsync(User.GetUserId(), sessionId, ct);
        return Ok(messages);
    }
}
