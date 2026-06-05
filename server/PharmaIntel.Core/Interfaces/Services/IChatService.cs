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
    // Benh nhan tao (hoac lay lai) phien chat voi mot duoc si cu the.
    // Moi cap (benh nhan, duoc si) la mot cuoc tro chuyen rieng.
    Task<ChatSessionDto> GetOrCreateSessionForUserAsync(long userId, long pharmacistId, CancellationToken ct = default);

    // Lay lich su tin nhan cua mot phien (sau khi da check quyen truy cap).
    Task<IReadOnlyList<ChatMessageDto>> GetMessagesAsync(long requesterUserId, long sessionId, CancellationToken ct = default);

    // Luu mot tin nhan vao DB. Tra ve DTO da co Id + SentAt de broadcast.
    // senderUserId la userId tu JWT; service tu suy ra sender_type (user/pharmacist).
    Task<ChatMessageDto> SaveMessageAsync(long senderUserId, long sessionId, string content, CancellationToken ct = default);

    // Kiem tra mot user co quyen tham gia phien khong (benh nhan so huu phien,
    // duoc si duoc gan, hoac duoc si chua gan nhung phien dang cho).
    Task<bool> CanAccessSessionAsync(long userId, long sessionId, CancellationToken ct = default);

    // Sinh tin tra loi cua AI cho phien (senderType "system"). Tra null neu phien
    // da co duoc si tiep quan (PharmacistId != null) hoac da dong -> luc do AI ngung.
    Task<ChatMessageDto?> GenerateAiReplyAsync(long sessionId, CancellationToken ct = default);

    // Danh sach phien cho duoc si: "waiting" = hang cho (AI dang xu ly, co the tiep quan),
    // "open" = cac phien duoc si nay da nhan. status null = ca hai.
    Task<IReadOnlyList<ChatSessionListItemDto>> GetSessionsForPharmacistAsync(
        long pharmacistUserId, string? status, CancellationToken ct = default);
}
