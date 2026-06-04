// =============================================================================
// Service: ChatService - nghiep vu chat benh nhan <-> duoc si.
// Trach nhiem:
//   - Tao/lay phien dang mo cho benh nhan.
//   - Luu tin nhan, tu suy ra sender_type tu vai tro nguoi gui trong phien.
//   - Kiem tra quyen truy cap phien (benh nhan so huu / duoc si).
// Khong broadcast o day - viec do la cua ChatHub (giu service thuan nghiep vu,
// de test va tai su dung cho REST fallback).
// =============================================================================
using Microsoft.EntityFrameworkCore;
using PharmaIntel.Core.DTOs.Chat;
using PharmaIntel.Core.Entities;
using PharmaIntel.Core.Exceptions;
using PharmaIntel.Core.Interfaces.Services;
using PharmaIntel.Infrastructure.Data;

namespace PharmaIntel.Infrastructure.Services;

public class ChatService : IChatService
{
    private readonly PharmaIntelDbContext _db;

    public ChatService(PharmaIntelDbContext db)
    {
        _db = db;
    }

    public async Task<ChatSessionDto> GetOrCreateSessionForUserAsync(long userId, CancellationToken ct = default)
    {
        var session = await _db.PharmacistChatSessions
            .Where(s => s.UserId == userId && (s.Status == "open" || s.Status == "waiting"))
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync(ct);

        if (session is null)
        {
            session = new PharmacistChatSession
            {
                UserId = userId,
                Status = "waiting", // cho duoc si nhan phien
                StartedAt = DateTime.UtcNow
            };
            _db.PharmacistChatSessions.Add(session);
            await _db.SaveChangesAsync(ct);
        }

        return ToSessionDto(session);
    }

    public async Task<IReadOnlyList<ChatMessageDto>> GetMessagesAsync(long requesterUserId, long sessionId, CancellationToken ct = default)
    {
        if (!await CanAccessSessionAsync(requesterUserId, sessionId, ct))
            throw new ForbiddenException("Khong co quyen xem phien chat nay");

        return await _db.PharmacistChatMessages
            .AsNoTracking()
            .Where(m => m.SessionId == sessionId)
            .OrderBy(m => m.SentAt)
            .Select(m => new ChatMessageDto
            {
                Id = m.Id,
                SessionId = m.SessionId,
                SenderType = m.SenderType,
                SenderUserId = m.SenderUserId,
                SenderPharmacistId = m.SenderPharmacistId,
                Content = m.Content,
                SentAt = m.SentAt
            })
            .ToListAsync(ct);
    }

    public async Task<ChatMessageDto> SaveMessageAsync(long senderUserId, long sessionId, string content, CancellationToken ct = default)
    {
        var session = await _db.PharmacistChatSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId, ct)
            ?? throw new NotFoundException("Khong tim thay phien chat");

        if (session.Status is "closed" or "cancelled")
            throw new ValidationException("sessionId", "Phien chat da dong");

        // Suy ra sender_type tu vai tro nguoi gui trong phien.
        var (senderType, senderPharmacistId) = await ResolveSenderAsync(senderUserId, session, ct);

        var message = new PharmacistChatMessage
        {
            SessionId = sessionId,
            SenderType = senderType,
            SenderUserId = senderType == "user" ? senderUserId : null,
            SenderPharmacistId = senderPharmacistId,
            Content = content.Trim(),
            SentAt = DateTime.UtcNow
        };

        _db.PharmacistChatMessages.Add(message);

        // Duoc si gui tin dau tien -> phien chuyen sang "open" + gan duoc si.
        if (senderType == "pharmacist" && session.Status == "waiting")
        {
            session.Status = "open";
            session.PharmacistId = senderPharmacistId;
        }

        await _db.SaveChangesAsync(ct);

        return new ChatMessageDto
        {
            Id = message.Id,
            SessionId = message.SessionId,
            SenderType = message.SenderType,
            SenderUserId = message.SenderUserId,
            SenderPharmacistId = message.SenderPharmacistId,
            Content = message.Content,
            SentAt = message.SentAt
        };
    }

    public async Task<bool> CanAccessSessionAsync(long userId, long sessionId, CancellationToken ct = default)
    {
        var session = await _db.PharmacistChatSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sessionId, ct);

        if (session is null) return false;

        // Benh nhan so huu phien.
        if (session.UserId == userId) return true;

        // Nguoi gui la duoc si: cho phep neu da duoc gan phien, HOAC phien dang
        // cho (waiting) va chua co duoc si nao nhan -> duoc si bat ky co the vao nhan.
        var pharmacist = await _db.Pharmacists
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive, ct);

        if (pharmacist is null) return false;

        return session.PharmacistId == pharmacist.Id
               || (session.PharmacistId is null && session.Status == "waiting");
    }

    // Tra ve (sender_type, pharmacistId|null) cho nguoi gui trong boi canh phien.
    private async Task<(string senderType, long? pharmacistId)> ResolveSenderAsync(
        long senderUserId, PharmacistChatSession session, CancellationToken ct)
    {
        if (session.UserId == senderUserId)
            return ("user", null);

        var pharmacist = await _db.Pharmacists
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == senderUserId && p.IsActive, ct);

        if (pharmacist is not null &&
            (session.PharmacistId == pharmacist.Id ||
             (session.PharmacistId is null && session.Status == "waiting")))
        {
            return ("pharmacist", pharmacist.Id);
        }

        throw new ForbiddenException("Khong co quyen gui tin trong phien nay");
    }

    private static ChatSessionDto ToSessionDto(PharmacistChatSession s) => new()
    {
        Id = s.Id,
        UserId = s.UserId,
        PharmacistId = s.PharmacistId,
        Status = s.Status,
        StartedAt = s.StartedAt,
        ClosedAt = s.ClosedAt
    };
}
