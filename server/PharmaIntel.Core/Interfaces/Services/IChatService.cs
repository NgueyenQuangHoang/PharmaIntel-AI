// =============================================================================
// Interface: IChatService
// Chuc nang: Hop dong cho chat benh nhan <-> duoc si.
//   - Tao/lay phien, lay lich su tin nhan (REST).
//   - Luu + tra ve tin nhan da persist (goi tu Hub khi co tin real-time).
// Phan quyen: kiem tra nguoi goi co thuoc phien khong duoc lam o tang service.
// =============================================================================
using PharmaIntel.Core.DTOs.Chat;

namespace PharmaIntel.Core.Interfaces.Services;

public interface IChatService
{
    // Benh nhan tao (hoac lay lai) phien dang mo cua minh.
    Task<ChatSessionDto> GetOrCreateSessionForUserAsync(long userId, CancellationToken ct = default);

    // Lay lich su tin nhan cua mot phien (sau khi da check quyen truy cap).
    Task<IReadOnlyList<ChatMessageDto>> GetMessagesAsync(long requesterUserId, long sessionId, CancellationToken ct = default);

    // Luu mot tin nhan vao DB. Tra ve DTO da co Id + SentAt de broadcast.
    // senderUserId la userId tu JWT; service tu suy ra sender_type (user/pharmacist).
    Task<ChatMessageDto> SaveMessageAsync(long senderUserId, long sessionId, string content, CancellationToken ct = default);

    // Kiem tra mot user co quyen tham gia phien khong (benh nhan so huu phien,
    // duoc si duoc gan, hoac duoc si chua gan nhung phien dang cho).
    Task<bool> CanAccessSessionAsync(long userId, long sessionId, CancellationToken ct = default);
}
