// =============================================================================
// DTOs: Chat (Chat real-time benh nhan <-> duoc si)
// Chuc nang: Hop dong du lieu cho REST (lich su, tao phien) va SignalR (broadcast).
// =============================================================================
namespace PharmaIntel.Core.DTOs.Chat;

// --- Tin nhan tra ve cho client (dung cho ca REST history lan SignalR push) ---
public class ChatMessageDto
{
    public long Id { get; set; }
    public long SessionId { get; set; }
    public string SenderType { get; set; } = string.Empty; // user | pharmacist | system
    public long? SenderUserId { get; set; }
    public long? SenderPharmacistId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}

// --- Phien chat ---
public class ChatSessionDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long? PharmacistId { get; set; }
    public string Status { get; set; } = "open"; // open | waiting | closed | cancelled
    public DateTime StartedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
}

// --- Body khi client gui tin (qua SignalR hoac REST fallback) ---
public class SendMessageRequest
{
    public long SessionId { get; set; }
    public string Content { get; set; } = string.Empty;
}

// --- Body khi benh nhan mo/tao phien chat voi mot duoc si cu the ---
public class StartChatSessionRequest
{
    public long PharmacistId { get; set; }
}

// --- Item danh sach phien cho duoc si (kem thong tin hien thi) ---
public class ChatSessionListItemDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // waiting | open
    public DateTime StartedAt { get; set; }
    public string? LastMessage { get; set; }
    public DateTime? LastMessageAt { get; set; }
}
