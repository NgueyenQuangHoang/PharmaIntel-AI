// =============================================================================
// Service: MedicationReminderService
// Chuc nang: Quan ly lich nhac thuoc + log lan nhac (user-scoped).
// Quan he: N:1 voi User | N:1 voi PrescriptionItem (optional) | 1:N voi MedicationReminderLog.
// Quy tac:
//   - PrescriptionItemId neu cung cap: phai thuoc don thuoc cua chinh user (cross-tenant
//     guard). Auto snapshot MedicationName tu PrescriptionItem.MedicationName neu user
//     khong nhap.
//   - Reminder status transition cho phep:
//       active     <-> paused
//       active     -> completed | cancelled
//       paused     -> completed | cancelled
//       completed  -> (terminal)
//       cancelled  -> (terminal)
//   - Khong cho them log khi reminder dang `cancelled` (giu data sach).
//   - Log POST chi cho status = taken/missed/skipped. CompletedAt:
//       taken/skipped -> set = UtcNow
//       missed        -> null (nguoi dung khong thuc hien)
//   - Cascade delete: xoa reminder -> EF cascade xoa logs.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.DTOs.MedicationReminders;
using PharmaIntel.Core.Entities;
using PharmaIntel.Core.Exceptions;
using PharmaIntel.Core.Interfaces.Services;
using PharmaIntel.Infrastructure.Data;

namespace PharmaIntel.Infrastructure.Services;

public class MedicationReminderService : IMedicationReminderService
{
    private readonly PharmaIntelDbContext _db;

    public MedicationReminderService(PharmaIntelDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<MedicationReminderListItemDto>> ListMyAsync(long userId, MedicationReminderListQuery q, CancellationToken ct = default)
    {
        q.Normalize();

        await AutoCompleteExpiredAsync(userId, ct);

        var query = _db.MedicationReminders.AsNoTracking().Where(r => r.UserId == userId);

        if (!string.IsNullOrWhiteSpace(q.Status))
            query = query.Where(r => r.Status == q.Status);

        if (!string.IsNullOrWhiteSpace(q.Q))
        {
            var k = q.Q.Trim().ToLower();
            query = query.Where(r => r.MedicationName.ToLower().Contains(k));
        }

        var total = await query.CountAsync(ct);

        // Range hom nay (UTC) de SQL co the dung index tren scheduled_at thay vi
        // CAST(scheduled_at AS DATE). EF dich thanh `WHERE scheduled_at >= @start AND scheduled_at < @end`.
        var todayStart = DateTime.UtcNow.Date;
        var tomorrowStart = todayStart.AddDays(1);

        var items = await query
            .OrderByDescending(r => r.Status == "active")
            .ThenBy(r => r.ReminderTime)
            .ThenByDescending(r => r.CreatedAt)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(r => new MedicationReminderListItemDto
            {
                Id = r.Id,
                PrescriptionItemId = r.PrescriptionItemId,
                MedicationName = r.MedicationName,
                FrequencyType = r.FrequencyType,
                ReminderTime = r.ReminderTime,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                Status = r.Status,
                LogCount = r.Logs.Count,
                TodayLogStatus = r.Logs
                    .Where(l => l.ScheduledAt >= todayStart && l.ScheduledAt < tomorrowStart)
                    .Select(l => l.Status)
                    .FirstOrDefault(),
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            })
            .ToListAsync(ct);

        return new PagedResult<MedicationReminderListItemDto>
        {
            Items = items,
            Page = q.Page,
            PageSize = q.PageSize,
            TotalCount = total
        };
    }

    public async Task<MedicationReminderDto> GetByIdAsync(long userId, long id, CancellationToken ct = default)
    {
        await AutoCompleteExpiredAsync(userId, ct);

        var r = await _db.MedicationReminders.AsNoTracking()
            .Include(x => x.Logs)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("lich nhac thuoc", id);
        if (r.UserId != userId)
            throw new ForbiddenException("Lich nhac khong thuoc ve ban");
        return ToDetailDto(r);
    }

    public async Task<MedicationReminderDto> CreateAsync(long userId, MedicationReminderCreateRequest req, CancellationToken ct = default)
    {
        var (prescriptionItemId, medicationName) = await ResolvePrescriptionItemAsync(userId, req.PrescriptionItemId, req.MedicationName, ct);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startDate = req.StartDate ?? today;
        ValidateDateRange(startDate, req.EndDate);

        var entity = new MedicationReminder
        {
            UserId = userId,
            PrescriptionItemId = prescriptionItemId,
            MedicationName = medicationName,
            FrequencyType = req.FrequencyType,
            ReminderTime = req.ReminderTime,
            StartDate = startDate,
            EndDate = req.EndDate,
            Status = "active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.MedicationReminders.Add(entity);
        await _db.SaveChangesAsync(ct);

        return ToDetailDto(entity);
    }

    public async Task<MedicationReminderDto> UpdateAsync(long userId, long id, MedicationReminderUpdateRequest req, CancellationToken ct = default)
    {
        var entity = await _db.MedicationReminders.FirstOrDefaultAsync(r => r.Id == id, ct)
                     ?? throw new NotFoundException("lich nhac thuoc", id);
        if (entity.UserId != userId)
            throw new ForbiddenException("Lich nhac khong thuoc ve ban");

        ValidateTransition(entity.Status, req.Status);

        var (prescriptionItemId, medicationName) = await ResolvePrescriptionItemAsync(userId, req.PrescriptionItemId, req.MedicationName, ct);

        var newStart = req.StartDate ?? entity.StartDate;
        ValidateDateRange(newStart, req.EndDate);

        entity.PrescriptionItemId = prescriptionItemId;
        entity.MedicationName = medicationName;
        entity.FrequencyType = req.FrequencyType;
        entity.ReminderTime = req.ReminderTime;
        entity.StartDate = newStart;
        entity.EndDate = req.EndDate;
        entity.Status = req.Status;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return ToDetailDto(entity);
    }

    public async Task DeleteAsync(long userId, long id, CancellationToken ct = default)
    {
        var entity = await _db.MedicationReminders.FirstOrDefaultAsync(r => r.Id == id, ct)
                     ?? throw new NotFoundException("lich nhac thuoc", id);
        if (entity.UserId != userId)
            throw new ForbiddenException("Lich nhac khong thuoc ve ban");

        _db.MedicationReminders.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<MedicationReminderLogDto> AddLogAsync(long userId, long reminderId, MedicationReminderLogCreateRequest req, CancellationToken ct = default)
    {
        var reminder = await _db.MedicationReminders.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == reminderId, ct)
            ?? throw new NotFoundException("lich nhac thuoc", reminderId);
        if (reminder.UserId != userId)
            throw new ForbiddenException("Lich nhac khong thuoc ve ban");
        // Chi cho log khi reminder dang chay. paused/completed/cancelled deu bi chan
        // o tang service - khong tin UI chi an nut "Da uong" tren tab active vi user
        // co the goi API truc tiep.
        if (reminder.Status != "active")
            throw new ConflictException($"Chi co the ghi nhan uong thuoc cho lich dang hoat dong (hien tai: {reminder.Status})");

        // ScheduledAt phai nam trong [StartDate, EndDate]. EndDate null = mo, khong gioi
        // han duoi. Tranh user log lui ve qua khu/tuong lai ngoai pham vi don thuoc.
        var scheduledDate = DateOnly.FromDateTime(req.ScheduledAt);
        if (scheduledDate < reminder.StartDate)
            throw new ConflictException("Thoi diem ghi nhan truoc ngay bat dau lich nhac");
        if (reminder.EndDate.HasValue && scheduledDate > reminder.EndDate.Value)
            throw new ConflictException("Thoi diem ghi nhan sau ngay ket thuc lich nhac");

        // Upsert theo (ReminderId, ScheduledAt). User co the bam "Da uong" nham roi
        // doi sang "Bo qua", hoac double-click - moi truong hop deu chi giu 1 log cho
        // moi slot gio nhac. Friendly hon hard-error 409.
        var existing = await _db.MedicationReminderLogs
            .FirstOrDefaultAsync(l => l.ReminderId == reminderId && l.ScheduledAt == req.ScheduledAt, ct);

        if (existing is not null)
        {
            existing.Status = req.Status;
            existing.CompletedAt = req.Status == "missed" ? null : DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return ToLogDto(existing);
        }

        var log = new MedicationReminderLog
        {
            ReminderId = reminderId,
            ScheduledAt = req.ScheduledAt,
            Status = req.Status,
            CompletedAt = req.Status == "missed" ? null : DateTime.UtcNow
        };

        _db.MedicationReminderLogs.Add(log);
        await _db.SaveChangesAsync(ct);

        return ToLogDto(log);
    }

    public async Task<PagedResult<MedicationReminderLogDto>> ListLogsAsync(long userId, long reminderId, MedicationReminderLogListQuery q, CancellationToken ct = default)
    {
        var reminder = await _db.MedicationReminders.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == reminderId, ct)
            ?? throw new NotFoundException("lich nhac thuoc", reminderId);
        if (reminder.UserId != userId)
            throw new ForbiddenException("Lich nhac khong thuoc ve ban");

        q.Normalize();
        var query = _db.MedicationReminderLogs.AsNoTracking().Where(l => l.ReminderId == reminderId);

        if (!string.IsNullOrWhiteSpace(q.Status))
            query = query.Where(l => l.Status == q.Status);
        if (q.FromDate.HasValue)
            query = query.Where(l => l.ScheduledAt >= q.FromDate.Value);
        if (q.ToDate.HasValue)
            query = query.Where(l => l.ScheduledAt <= q.ToDate.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(l => l.ScheduledAt)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(l => new MedicationReminderLogDto
            {
                Id = l.Id,
                ReminderId = l.ReminderId,
                ScheduledAt = l.ScheduledAt,
                CompletedAt = l.CompletedAt,
                Status = l.Status
            })
            .ToListAsync(ct);

        return new PagedResult<MedicationReminderLogDto>
        {
            Items = items,
            Page = q.Page,
            PageSize = q.PageSize,
            TotalCount = total
        };
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private async Task<(long? PrescriptionItemId, string MedicationName)> ResolvePrescriptionItemAsync(
        long userId, long? prescriptionItemId, string? medicationName, CancellationToken ct)
    {
        if (prescriptionItemId.HasValue)
        {
            // Cross-tenant guard: PrescriptionItem phai thuoc Prescription cua chinh user.
            var item = await _db.PrescriptionItems.AsNoTracking()
                .Include(i => i.Prescription)
                .FirstOrDefaultAsync(i => i.Id == prescriptionItemId.Value, ct)
                ?? throw new NotFoundException("muc don thuoc", prescriptionItemId.Value);

            if (item.Prescription.UserId != userId)
                throw new ForbiddenException("Muc don thuoc khong thuoc ve ban");

            var name = string.IsNullOrWhiteSpace(medicationName) ? item.MedicationName : medicationName.Trim();
            return (item.Id, name);
        }

        var trimmed = medicationName?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(trimmed))
            throw new ValidationException("medicationName", "MedicationName la bat buoc khi khong link toi don thuoc");
        return (null, trimmed);
    }

    // Auto-complete reminder qua han: EndDate < today va Status = 'active'.
    // Goi truoc moi List/Get de user nhin thay state moi nhat. Cost re vi co WHERE
    // index-friendly + chi update khi co row du dieu kien.
    private async Task AutoCompleteExpiredAsync(long userId, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var nowUtc = DateTime.UtcNow;
        await _db.MedicationReminders
            .Where(r => r.UserId == userId
                     && r.Status == "active"
                     && r.EndDate != null
                     && r.EndDate < today)
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.Status, "completed")
                .SetProperty(r => r.UpdatedAt, nowUtc), ct);
    }

    private static void ValidateDateRange(DateOnly startDate, DateOnly? endDate)
    {
        if (endDate.HasValue && endDate.Value < startDate)
            throw new ValidationException("endDate", "EndDate phai >= StartDate");
    }

    private static void ValidateTransition(string current, string next)
    {
        if (current == next) return;

        var allowed = current switch
        {
            "active" => new[] { "paused", "completed", "cancelled" },
            "paused" => new[] { "active", "completed", "cancelled" },
            _ => Array.Empty<string>()
        };

        if (!allowed.Contains(next))
            throw new ConflictException($"Khong the chuyen tu '{current}' sang '{next}'");
    }

    private static MedicationReminderDto ToDetailDto(MedicationReminder r)
    {
        var todayStart = DateTime.UtcNow.Date;
        var tomorrowStart = todayStart.AddDays(1);
        return new MedicationReminderDto
        {
            Id = r.Id,
            PrescriptionItemId = r.PrescriptionItemId,
            MedicationName = r.MedicationName,
            FrequencyType = r.FrequencyType,
            ReminderTime = r.ReminderTime,
            StartDate = r.StartDate,
            EndDate = r.EndDate,
            Status = r.Status,
            LogCount = r.Logs?.Count ?? 0,
            TodayLogStatus = r.Logs?
                .Where(l => l.ScheduledAt >= todayStart && l.ScheduledAt < tomorrowStart)
                .Select(l => l.Status)
                .FirstOrDefault(),
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        };
    }

    private static MedicationReminderLogDto ToLogDto(MedicationReminderLog l) => new()
    {
        Id = l.Id,
        ReminderId = l.ReminderId,
        ScheduledAt = l.ScheduledAt,
        CompletedAt = l.CompletedAt,
        Status = l.Status
    };
}
