// =============================================================================
// Hub: ChatHub (SignalR) - kenh real-time chat benh nhan <-> duoc si.
// Luong:
//   1. Client ket noi /hubs/chat kem JWT (?access_token=... cho WebSocket).
//   2. Client goi JoinSession(sessionId) -> server check quyen -> add vao group.
//   3. Client goi SendMessage(sessionId, content) -> luu DB -> broadcast cho group.
//   4. Moi tin gui ve client qua event "ReceiveMessage".
// Group name: "session-{sessionId}" - chi thanh vien phien nhan duoc tin.
// Bao mat: [Authorize] -> moi connection phai co JWT hop le. Quyen truy cap
//          tung phien check qua IChatService (khong tin sessionId tu client).
// =============================================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PharmaIntel.API.Extensions;
using PharmaIntel.Core.Interfaces.Services;

namespace PharmaIntel.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;

    public ChatHub(IChatService chatService)
    {
        _chatService = chatService;
    }

    private static string GroupName(long sessionId) => $"session-{sessionId}";

    // Client goi sau khi ket noi de tham gia mot phien cu the.
    public async Task JoinSession(long sessionId)
    {
        var userId = Context.User!.GetUserId();
        if (!await _chatService.CanAccessSessionAsync(userId, sessionId, Context.ConnectionAborted))
            throw new HubException("Khong co quyen truy cap phien chat nay");

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(sessionId));
    }

    public async Task LeaveSession(long sessionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(sessionId));
    }

    // Client gui tin nhan. Server luu DB roi broadcast cho ca group (gom ca nguoi gui).
    public async Task SendMessage(long sessionId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new HubException("Noi dung tin nhan trong");

        var userId = Context.User!.GetUserId();

        // SaveMessageAsync tu kiem tra quyen + suy ra sender_type, nem neu khong hop le.
        var message = await _chatService.SaveMessageAsync(userId, sessionId, content, Context.ConnectionAborted);

        await Clients.Group(GroupName(sessionId)).SendAsync("ReceiveMessage", message, Context.ConnectionAborted);
    }
}
