// =============================================================================
// Service: NotificationService
// Chuc nang: User-scoped read + mark + delete cho thong bao.
// Quy tac:
//   - User KHONG tu tao notification - system tao qua module noi bo (order event,
//     reminder, prescription_new, diagnostic_complete, promotion, system).
//   - Filter mac dinh theo userId; truy cap noti cua user khac -> 403.
//   - MarkRead idempotent: noti da doc thi giu nguyen ReadAt cu, khong update.
//   - MarkAllRead bulk update qua ExecuteUpdateAsync (1 round-trip).
//   - Index (UserId, IsRead) toi uu list + count chua doc.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.DTOs.Notifications;
using PharmaIntel.Core.Entities;
using PharmaIntel.Core.Exceptions;
using PharmaIntel.Core.Interfaces.Services;
using PharmaIntel.Infrastructure.Data;

namespace PharmaIntel.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly PharmaIntelDbContext _db;

    public NotificationService(PharmaIntelDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<NotificationDto>> ListMyAsync(long userId, NotificationListQuery q, CancellationToken ct = default)
    {
        q.Normalize();

        var query = _db.Notifications.AsNoTracking().Where(n => n.UserId == userId);

        if (q.IsRead.HasValue)
            query = query.Where(n => n.IsRead == q.IsRead);
        if (!string.IsNullOrWhiteSpace(q.Type))
            query = query.Where(n => n.NotificationType == q.Type);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                NotificationType = n.NotificationType,
                Title = n.Title,
                Body = n.Body,
                ReferenceType = n.ReferenceType,
                ReferenceId = n.ReferenceId,
                IsRead = n.IsRead,
                ReadAt = n.ReadAt,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync(ct);

        return new PagedResult<NotificationDto>
        {
            Items = items,
            Page = q.Page,
            PageSize = q.PageSize,
            TotalCount = total
        };
    }

    public async Task<UnreadCountDto> GetUnreadCountAsync(long userId, CancellationToken ct = default)
    {
        var count = await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .CountAsync(ct);
        return new UnreadCountDto { Count = count };
    }

    public async Task<NotificationDto> MarkReadAsync(long userId, long id, CancellationToken ct = default)
    {
        var entity = await _db.Notifications.FirstOrDefaultAsync(n => n.Id == id, ct)
                     ?? throw new NotFoundException("thong bao", id);
        if (entity.UserId != userId)
            throw new ForbiddenException("Thong bao khong thuoc ve ban");

        if (!entity.IsRead)
        {
            entity.IsRead = true;
            entity.ReadAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        return ToDto(entity);
    }

    public async Task<MarkAllReadResultDto> MarkAllReadAsync(long userId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var updated = await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAt, now), ct);

        return new MarkAllReadResultDto { UpdatedCount = updated };
    }

    public async Task DeleteAsync(long userId, long id, CancellationToken ct = default)
    {
        var entity = await _db.Notifications.FirstOrDefaultAsync(n => n.Id == id, ct)
                     ?? throw new NotFoundException("thong bao", id);
        if (entity.UserId != userId)
            throw new ForbiddenException("Thong bao khong thuoc ve ban");

        _db.Notifications.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    private static NotificationDto ToDto(Notification n) => new()
    {
        Id = n.Id,
        NotificationType = n.NotificationType,
        Title = n.Title,
        Body = n.Body,
        ReferenceType = n.ReferenceType,
        ReferenceId = n.ReferenceId,
        IsRead = n.IsRead,
        ReadAt = n.ReadAt,
        CreatedAt = n.CreatedAt
    };
}
