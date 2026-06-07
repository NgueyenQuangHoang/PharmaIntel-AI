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
    private readonly IDiagnosticEngine _engine;
    private readonly IAiMedicationRetrievalService _retrieval;

    public ChatService(
        PharmaIntelDbContext db,
        IDiagnosticEngine engine,
        IAiMedicationRetrievalService retrieval)
    {
        _db = db;
        _engine = engine;
        _retrieval = retrieval;
    }

    public async Task<ChatSessionDto> GetOrCreateSessionForUserAsync(long userId, long pharmacistId, CancellationToken ct = default)
    {
        // Moi cap (benh nhan, duoc si) la mot cuoc tro chuyen rieng.
        var pharmacist = await _db.Pharmacists
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == pharmacistId && p.IsActive, ct)
            ?? throw new NotFoundException("Khong tim thay duoc si");

        var session = await _db.PharmacistChatSessions
            .Where(s => s.UserId == userId && s.PharmacistId == pharmacistId
                        && (s.Status == "open" || s.Status == "waiting"))
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync(ct);

        if (session is null)
        {
            session = new PharmacistChatSession
            {
                UserId = userId,
                PharmacistId = pharmacist.Id, // gan duoc si dich danh ngay tu dau
                Status = "waiting",           // cho dung duoc si nay tiep quan
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

        // Duoc si dich danh gui tin dau tien -> tiep quan phien (waiting -> open).
        // PharmacistId da gan tu luc tao phien nen khong can gan lai.
        if (senderType == "pharmacist" && session.Status == "waiting")
            session.Status = "open";

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

        // Phien luon gan dich danh mot duoc si -> chi dung duoc si do duoc truy cap.
        var pharmacist = await _db.Pharmacists
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive, ct);

        if (pharmacist is null) return false;

        return session.PharmacistId == pharmacist.Id;
    }

    public async Task<ChatMessageDto?> GenerateAiReplyAsync(long sessionId, CancellationToken ct = default)
    {
        var session = await _db.PharmacistChatSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId, ct);

        // AI chi rep khi phien con "waiting" (duoc si chua tiep quan -> chua "open").
        if (session is null || session.Status != "waiting")
            return null;

        // Chi rep DUNG MOT LAN dau tien: neu da co tin AI (system) thi thoi.
        var aiAlreadyReplied = await _db.PharmacistChatMessages
            .AnyAsync(m => m.SessionId == sessionId && m.SenderType == "system", ct);
        if (aiAlreadyReplied) return null;

        // Lay 20 tin gan nhat lam ngu canh; tin user moi nhat la cau hoi can tra loi.
        var recent = await _db.PharmacistChatMessages
            .AsNoTracking()
            .Where(m => m.SessionId == sessionId)
            .OrderByDescending(m => m.SentAt)
            .Take(20)
            .ToListAsync(ct);
        recent.Reverse();

        var latestUser = recent.LastOrDefault(m => m.SenderType == "user");
        if (latestUser is null) return null; // khong co cau hoi cua user -> khong rep

        var history = recent
            .Where(m => m.Id != latestUser.Id)
            .Select(m => $"{(m.SenderType == "user" ? "Khach" : "AI")}: {m.Content}")
            .ToList();

        var meds = await _retrieval.SearchRelevantMedicationsAsync(
            symptomsSummary: string.Empty,
            conversationMessages: history,
            ct);

        var reply = await _engine.GenerateChatReplyAsync(
            symptomsSummary: string.Empty,
            conversationMessages: history,
            userMessage: latestUser.Content,
            medicationContexts: meds,
            ct);

        if (string.IsNullOrWhiteSpace(reply)) return null;

        var message = new PharmacistChatMessage
        {
            SessionId = sessionId,
            SenderType = "system", // AI = system (sender ids deu null theo CHECK constraint)
            SenderUserId = null,
            SenderPharmacistId = null,
            Content = reply.Trim(),
            SentAt = DateTime.UtcNow
        };

        _db.PharmacistChatMessages.Add(message);
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

    public async Task<IReadOnlyList<ChatSessionListItemDto>> GetSessionsForPharmacistAsync(
        long pharmacistUserId, string? status, CancellationToken ct = default)
    {
        var pharmacist = await _db.Pharmacists
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == pharmacistUserId && p.IsActive, ct);

        if (pharmacist is null) return [];

        // Phien luon gan dich danh duoc si. waiting = hang cho (AI da rep, cho minh
        // tiep quan); open = minh da tiep quan.
        var query = _db.PharmacistChatSessions.AsNoTracking().Where(s =>
            s.PharmacistId == pharmacist.Id && (s.Status == "waiting" || s.Status == "open"));

        if (status == "waiting")
            query = query.Where(s => s.Status == "waiting");
        else if (status == "open")
            query = query.Where(s => s.Status == "open");

        return await query
            .OrderByDescending(s => s.StartedAt)
            .Select(s => new ChatSessionListItemDto
            {
                Id = s.Id,
                UserId = s.UserId,
                UserFullName = s.User.FullName,
                Status = s.Status,
                StartedAt = s.StartedAt,
                LastMessage = s.Messages.OrderByDescending(m => m.SentAt).Select(m => m.Content).FirstOrDefault(),
                LastMessageAt = s.Messages.OrderByDescending(m => m.SentAt).Select(m => (DateTime?)m.SentAt).FirstOrDefault()
            })
            .ToListAsync(ct);
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

        if (pharmacist is not null && session.PharmacistId == pharmacist.Id)
            return ("pharmacist", pharmacist.Id);

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
