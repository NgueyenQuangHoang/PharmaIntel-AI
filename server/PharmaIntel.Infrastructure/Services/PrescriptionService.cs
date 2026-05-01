// =============================================================================
// Service: PrescriptionService
// Chuc nang: Quan ly don thuoc + items (user-scoped).
// Quan he: N:1 voi User | 1:N voi PrescriptionItem | 1:N voi Order (don hang co the
//          tham chieu prescription neu thuoc keo don).
// Quy tac nghiep vu:
//   - Prescription moi tao = status "draft" mac dinh.
//   - Items chi them/sua/xoa khi prescription dang "draft" (khoa khi active/completed/...).
//   - Status transition cho phep:
//       draft     -> active | cancelled
//       active    -> completed | cancelled
//       completed -> (terminal)
//       cancelled -> (terminal)
//     Khong cho user tu set "expired" (do system tu chuyen theo PrescribedDate sau nay).
//   - Khi chuyen draft -> active, prescription phai co it nhat 1 item.
//   - Delete: chi xoa duoc khi chua co Order tham chieu (ConflictException neu da co).
//   - DoctorId neu co phai ton tai + IsActive; auto snapshot Doctor.FullName.
//   - MedicationId neu co phai ton tai + IsActive; auto snapshot Medication.Name.
//   - VerificationStatus do pharmacist quan ly o module khac, khong expose o API nay.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.DTOs.Prescriptions;
using PharmaIntel.Core.Entities;
using PharmaIntel.Core.Exceptions;
using PharmaIntel.Core.Interfaces.Services;
using PharmaIntel.Infrastructure.Data;

namespace PharmaIntel.Infrastructure.Services;

public class PrescriptionService : IPrescriptionService
{
    private readonly PharmaIntelDbContext _db;

    public PrescriptionService(PharmaIntelDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<PrescriptionListItemDto>> ListMyAsync(long userId, PrescriptionListQuery q, CancellationToken ct = default)
    {
        q.Normalize();

        var query = _db.Prescriptions.AsNoTracking().Where(p => p.UserId == userId);

        if (!string.IsNullOrWhiteSpace(q.Status))
            query = query.Where(p => p.Status == q.Status);

        if (!string.IsNullOrWhiteSpace(q.Q))
        {
            var k = q.Q.Trim().ToLower();
            query = query.Where(p =>
                (p.Title != null && p.Title.ToLower().Contains(k))
                || (p.DoctorNameSnapshot != null && p.DoctorNameSnapshot.ToLower().Contains(k)));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(p => new PrescriptionListItemDto
            {
                Id = p.Id,
                DoctorId = p.DoctorId,
                DoctorNameSnapshot = p.DoctorNameSnapshot,
                Title = p.Title,
                PrescribedDate = p.PrescribedDate,
                Status = p.Status,
                VerificationStatus = p.VerificationStatus,
                ItemCount = p.Items.Count,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync(ct);

        return new PagedResult<PrescriptionListItemDto>
        {
            Items = items,
            Page = q.Page,
            PageSize = q.PageSize,
            TotalCount = total
        };
    }

    public async Task<PrescriptionDto> GetByIdAsync(long userId, long id, CancellationToken ct = default)
    {
        var p = await _db.Prescriptions.AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("don thuoc", id);

        if (p.UserId != userId)
            throw new ForbiddenException("Don thuoc khong thuoc ve ban");

        return ToDetailDto(p);
    }

    public async Task<PrescriptionDto> CreateAsync(long userId, PrescriptionCreateRequest req, CancellationToken ct = default)
    {
        var doctorSnapshot = req.DoctorNameSnapshot?.Trim();

        if (req.DoctorId.HasValue)
        {
            var doctor = await _db.Doctors.AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == req.DoctorId.Value, ct)
                ?? throw new NotFoundException("bac si", req.DoctorId.Value);
            if (!doctor.IsActive)
                throw new ConflictException("Bac si khong con hoat dong");
            doctorSnapshot = string.IsNullOrWhiteSpace(doctorSnapshot) ? doctor.FullName : doctorSnapshot;
        }

        var entity = new Prescription
        {
            UserId = userId,
            DoctorId = req.DoctorId,
            DoctorNameSnapshot = doctorSnapshot,
            Title = req.Title?.Trim(),
            PrescribedDate = req.PrescribedDate,
            Status = "draft",
            VerificationStatus = "not_required",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Prescriptions.Add(entity);
        await _db.SaveChangesAsync(ct);

        return ToDetailDto(entity);
    }

    public async Task<PrescriptionDto> UpdateAsync(long userId, long id, PrescriptionUpdateRequest req, CancellationToken ct = default)
    {
        var entity = await _db.Prescriptions
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException("don thuoc", id);

        if (entity.UserId != userId)
            throw new ForbiddenException("Don thuoc khong thuoc ve ban");

        // Validate status transition
        ValidateTransition(entity.Status, req.Status, entity.Items.Count);

        // Validate doctor neu co
        var doctorSnapshot = req.DoctorNameSnapshot?.Trim();
        if (req.DoctorId.HasValue)
        {
            var doctor = await _db.Doctors.AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == req.DoctorId.Value, ct)
                ?? throw new NotFoundException("bac si", req.DoctorId.Value);
            if (!doctor.IsActive)
                throw new ConflictException("Bac si khong con hoat dong");
            doctorSnapshot = string.IsNullOrWhiteSpace(doctorSnapshot) ? doctor.FullName : doctorSnapshot;
        }

        entity.DoctorId = req.DoctorId;
        entity.DoctorNameSnapshot = doctorSnapshot;
        entity.Title = req.Title?.Trim();
        entity.PrescribedDate = req.PrescribedDate;
        entity.Status = req.Status;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return ToDetailDto(entity);
    }

    public async Task DeleteAsync(long userId, long id, CancellationToken ct = default)
    {
        var entity = await _db.Prescriptions.FirstOrDefaultAsync(p => p.Id == id, ct)
                     ?? throw new NotFoundException("don thuoc", id);
        if (entity.UserId != userId)
            throw new ForbiddenException("Don thuoc khong thuoc ve ban");

        var hasOrders = await _db.Orders.AnyAsync(o => o.PrescriptionId == id, ct);
        if (hasOrders)
            throw new ConflictException("Khong the xoa - don thuoc dang duoc tham chieu boi don hang");

        _db.Prescriptions.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<PrescriptionItemDto> AddItemAsync(long userId, long prescriptionId, PrescriptionItemCreateRequest req, CancellationToken ct = default)
    {
        var rx = await GetDraftForItemMutationAsync(userId, prescriptionId, ct);

        var (medicationId, medicationName) = await ResolveMedicationAsync(req.MedicationId, req.MedicationName, ct);

        var item = new PrescriptionItem
        {
            PrescriptionId = rx.Id,
            MedicationId = medicationId,
            MedicationName = medicationName,
            Dosage = req.Dosage?.Trim(),
            Frequency = req.Frequency?.Trim(),
            Duration = req.Duration?.Trim()
        };

        _db.PrescriptionItems.Add(item);
        rx.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return ToItemDto(item);
    }

    public async Task<PrescriptionItemDto> UpdateItemAsync(long userId, long prescriptionId, long itemId, PrescriptionItemUpdateRequest req, CancellationToken ct = default)
    {
        var rx = await GetDraftForItemMutationAsync(userId, prescriptionId, ct);

        var item = await _db.PrescriptionItems.FirstOrDefaultAsync(i => i.Id == itemId, ct)
                   ?? throw new NotFoundException("muc don thuoc", itemId);
        if (item.PrescriptionId != rx.Id)
            throw new NotFoundException("muc don thuoc", itemId);

        var (medicationId, medicationName) = await ResolveMedicationAsync(req.MedicationId, req.MedicationName, ct);

        item.MedicationId = medicationId;
        item.MedicationName = medicationName;
        item.Dosage = req.Dosage?.Trim();
        item.Frequency = req.Frequency?.Trim();
        item.Duration = req.Duration?.Trim();
        rx.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return ToItemDto(item);
    }

    public async Task RemoveItemAsync(long userId, long prescriptionId, long itemId, CancellationToken ct = default)
    {
        var rx = await GetDraftForItemMutationAsync(userId, prescriptionId, ct);

        var item = await _db.PrescriptionItems.FirstOrDefaultAsync(i => i.Id == itemId, ct)
                   ?? throw new NotFoundException("muc don thuoc", itemId);
        if (item.PrescriptionId != rx.Id)
            throw new NotFoundException("muc don thuoc", itemId);

        _db.PrescriptionItems.Remove(item);
        rx.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private async Task<Prescription> GetDraftForItemMutationAsync(long userId, long prescriptionId, CancellationToken ct)
    {
        var rx = await _db.Prescriptions.FirstOrDefaultAsync(p => p.Id == prescriptionId, ct)
                 ?? throw new NotFoundException("don thuoc", prescriptionId);
        if (rx.UserId != userId)
            throw new ForbiddenException("Don thuoc khong thuoc ve ban");
        if (rx.Status != "draft")
            throw new ConflictException($"Khong the chinh sua items khi don thuoc dang o trang thai '{rx.Status}'");
        return rx;
    }

    private async Task<(long? MedicationId, string MedicationName)> ResolveMedicationAsync(
        long? medicationId, string? medicationName, CancellationToken ct)
    {
        if (medicationId.HasValue)
        {
            var med = await _db.Medications.AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == medicationId.Value, ct)
                ?? throw new NotFoundException("thuoc", medicationId.Value);
            if (!med.IsActive)
                throw new ConflictException($"Thuoc '{med.Name}' khong con kinh doanh");
            // Snapshot ten thuoc luc them - khong phu thuoc rename ve sau
            var name = string.IsNullOrWhiteSpace(medicationName) ? med.Name : medicationName.Trim();
            return (med.Id, name);
        }

        // Free-text
        var trimmed = medicationName?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(trimmed))
            throw new ValidationException("medicationName", "MedicationName la bat buoc khi khong chon thuoc tu danh muc");
        return (null, trimmed);
    }

    private static void ValidateTransition(string current, string next, int itemCount)
    {
        if (current == next) return;

        var allowed = current switch
        {
            "draft" => new[] { "active", "cancelled" },
            "active" => new[] { "completed", "cancelled" },
            _ => Array.Empty<string>()
        };

        if (!allowed.Contains(next))
            throw new ConflictException($"Khong the chuyen tu '{current}' sang '{next}'");

        if (current == "draft" && next == "active" && itemCount == 0)
            throw new ConflictException("Don thuoc phai co it nhat 1 muc thuoc truoc khi kich hoat");
    }

    private static PrescriptionItemDto ToItemDto(PrescriptionItem i) => new()
    {
        Id = i.Id,
        PrescriptionId = i.PrescriptionId,
        MedicationId = i.MedicationId,
        MedicationName = i.MedicationName,
        Dosage = i.Dosage,
        Frequency = i.Frequency,
        Duration = i.Duration
    };

    private static PrescriptionDto ToDetailDto(Prescription p) => new()
    {
        Id = p.Id,
        DoctorId = p.DoctorId,
        DoctorNameSnapshot = p.DoctorNameSnapshot,
        Title = p.Title,
        PrescribedDate = p.PrescribedDate,
        Status = p.Status,
        VerificationStatus = p.VerificationStatus,
        ItemCount = p.Items?.Count ?? 0,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
        Items = (p.Items ?? new List<PrescriptionItem>()).Select(ToItemDto).ToList()
    };
}
