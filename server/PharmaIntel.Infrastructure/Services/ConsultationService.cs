// =============================================================================
// Service: ConsultationService
// Chuc nang: Tao yeu cau tu van + check trung lich + duoc si duyet/tu choi.
// Quy tac:
//   - Khong cho dat lich qua khu.
//   - Trung lich neu cung pharmacist co consultation o status pending/accepted
//     cach scheduled_at < 30 phut.
//   - Chi duoc si chu so huu (UserId tro toi Pharmacist) moi cap nhat status.
//   - Transition hop le: pending -> accepted | rejected | cancelled
//                       accepted -> completed | cancelled
// =============================================================================
using Microsoft.EntityFrameworkCore;
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.DTOs.Consultations;
using PharmaIntel.Core.Entities;
using PharmaIntel.Core.Exceptions;
using PharmaIntel.Core.Interfaces.Services;
using PharmaIntel.Infrastructure.Data;

namespace PharmaIntel.Infrastructure.Services;

public class ConsultationService : IConsultationService
{
    private const int ConflictMinutes = 30;
    private static readonly string[] BlockingStatuses = ["pending", "accepted"];
    private static readonly string[] AllowedStatuses = ["accepted", "rejected", "completed", "cancelled"];

    private readonly PharmaIntelDbContext _db;

    public ConsultationService(PharmaIntelDbContext db)
    {
        _db = db;
    }

    public async Task<ConsultationDto> CreateAsync(long userId, CreateConsultationRequest req, CancellationToken ct = default)
    {
        if (req.PharmacistId <= 0)
            throw new ValidationException("pharmacistId", "Thieu duoc si");

        var scheduled = DateTime.SpecifyKind(req.ScheduledAt, DateTimeKind.Utc);
        if (scheduled <= DateTime.UtcNow)
            throw new ValidationException("scheduledAt", "Thoi gian dat lich phai o tuong lai");

        var pharmacist = await _db.Pharmacists.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == req.PharmacistId && p.IsActive, ct)
            ?? throw new NotFoundException("duoc si", req.PharmacistId);

        var lower = scheduled.AddMinutes(-ConflictMinutes);
        var upper = scheduled.AddMinutes(ConflictMinutes);

        var conflict = await _db.Consultations.AsNoTracking().AnyAsync(c =>
            c.PharmacistId == req.PharmacistId
            && BlockingStatuses.Contains(c.Status)
            && c.ScheduledAt > lower
            && c.ScheduledAt < upper, ct);

        if (conflict)
            throw new ConflictException("Khung gio nay da co lich tu van khac, vui long chon thoi gian khac.");

        var entity = new Consultation
        {
            UserId = userId,
            PharmacistId = req.PharmacistId,
            ScheduledAt = scheduled,
            Note = string.IsNullOrWhiteSpace(req.Note) ? null : req.Note.Trim(),
            Status = "pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Consultations.Add(entity);
        await _db.SaveChangesAsync(ct);

        return await GetDtoAsync(entity.Id, ct);
    }

    public async Task<PagedResult<ConsultationDto>> ListForUserAsync(long userId, ListConsultationsQuery q, CancellationToken ct = default)
    {
        q.Normalize();
        var query = _db.Consultations.AsNoTracking().Where(c => c.UserId == userId);
        if (!string.IsNullOrWhiteSpace(q.Status))
            query = query.Where(c => c.Status == q.Status);
        return await PageAsync(query.OrderByDescending(c => c.ScheduledAt), q, ct);
    }

    public async Task<PagedResult<ConsultationDto>> ListForPharmacistAsync(long pharmacistUserId, ListConsultationsQuery q, CancellationToken ct = default)
    {
        q.Normalize();
        var pharmacist = await _db.Pharmacists.AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == pharmacistUserId, ct)
            ?? throw new NotFoundException("Khong tim thay ho so duoc si cua tai khoan nay");

        var query = _db.Consultations.AsNoTracking().Where(c => c.PharmacistId == pharmacist.Id);
        if (!string.IsNullOrWhiteSpace(q.Status))
            query = query.Where(c => c.Status == q.Status);

        return await PageAsync(query.OrderByDescending(c => c.ScheduledAt), q, ct);
    }

    public async Task<ConsultationDto> UpdateStatusAsync(long pharmacistUserId, long consultationId, UpdateConsultationStatusRequest req, CancellationToken ct = default)
    {
        if (!AllowedStatuses.Contains(req.Status))
            throw new ValidationException("status", "Trang thai khong hop le");

        var entity = await _db.Consultations.FirstOrDefaultAsync(c => c.Id == consultationId, ct)
            ?? throw new NotFoundException("lich tu van", consultationId);

        var pharmacist = await _db.Pharmacists.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == entity.PharmacistId, ct)
            ?? throw new NotFoundException("duoc si", entity.PharmacistId);

        if (pharmacist.UserId != pharmacistUserId)
            throw new ForbiddenException("Ban khong co quyen cap nhat lich tu van nay");

        if (!IsValidTransition(entity.Status, req.Status))
            throw new ConflictException($"Khong the chuyen trang thai tu '{entity.Status}' sang '{req.Status}'");

        entity.Status = req.Status;
        if (!string.IsNullOrWhiteSpace(req.ResponseNote))
            entity.ResponseNote = req.ResponseNote.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return await GetDtoAsync(entity.Id, ct);
    }

    private static bool IsValidTransition(string from, string to) => from switch
    {
        "pending" => to is "accepted" or "rejected" or "cancelled",
        "accepted" => to is "completed" or "cancelled",
        _ => false
    };

    private async Task<PagedResult<ConsultationDto>> PageAsync(IOrderedQueryable<Consultation> query, ListConsultationsQuery q, CancellationToken ct)
    {
        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(c => new ConsultationDto
            {
                Id = c.Id,
                UserId = c.UserId,
                UserFullName = c.User.FullName,
                UserEmail = c.User.Email,
                UserPhoneNumber = c.User.PhoneNumber,
                PharmacistId = c.PharmacistId,
                PharmacistName = c.Pharmacist.FullName,
                ScheduledAt = c.ScheduledAt,
                Note = c.Note,
                Status = c.Status,
                ResponseNote = c.ResponseNote,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync(ct);

        return new PagedResult<ConsultationDto>
        {
            Items = items,
            Page = q.Page,
            PageSize = q.PageSize,
            TotalCount = total
        };
    }

    private async Task<ConsultationDto> GetDtoAsync(long id, CancellationToken ct)
    {
        return await _db.Consultations.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new ConsultationDto
            {
                Id = c.Id,
                UserId = c.UserId,
                UserFullName = c.User.FullName,
                UserEmail = c.User.Email,
                UserPhoneNumber = c.User.PhoneNumber,
                PharmacistId = c.PharmacistId,
                PharmacistName = c.Pharmacist.FullName,
                ScheduledAt = c.ScheduledAt,
                Note = c.Note,
                Status = c.Status,
                ResponseNote = c.ResponseNote,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .FirstAsync(ct);
    }
}
