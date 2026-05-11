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
using Microsoft.Extensions.Hosting;
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.DTOs.Prescriptions;
using PharmaIntel.Core.Entities;
using PharmaIntel.Core.Exceptions;
using PharmaIntel.Core.Interfaces.Services;
using PharmaIntel.Infrastructure.Data;

namespace PharmaIntel.Infrastructure.Services;

public class PrescriptionService : IPrescriptionService
{
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB
    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };
    private static readonly HashSet<string> AllowedContentTypes =
        new(StringComparer.OrdinalIgnoreCase)
        { "image/jpeg", "image/png", "image/webp", "application/pdf" };

    private readonly PharmaIntelDbContext _db;
    private readonly IHostEnvironment _env;

    public PrescriptionService(PharmaIntelDbContext db, IHostEnvironment env)
    {
        _db = db;
        _env = env;
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
    // Document upload / list
    // -------------------------------------------------------------------------

    public async Task<PrescriptionDocumentDto> UploadDocumentAsync(
        long userId,
        long prescriptionId,
        Stream content,
        string fileName,
        string contentType,
        long lengthBytes,
        CancellationToken ct = default)
    {
        // 1. Validate file metadata
        if (content is null || lengthBytes <= 0)
            throw new ValidationException("file", "File khong duoc rong");
        if (lengthBytes > MaxFileSize)
            throw new ValidationException("file", "File vuot qua 10MB");
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ValidationException("file", "Thieu ten file");

        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
            throw new ValidationException("file",
                $"Extension '{ext}' khong duoc ho tro. Cho phep: {string.Join(", ", AllowedExtensions)}");

        if (!string.IsNullOrEmpty(contentType) && !AllowedContentTypes.Contains(contentType))
            throw new ValidationException("file",
                $"ContentType '{contentType}' khong duoc ho tro");

        // 2. Load prescription with ownership check
        var rx = await _db.Prescriptions.FirstOrDefaultAsync(p => p.Id == prescriptionId, ct)
                 ?? throw new NotFoundException("don thuoc", prescriptionId);
        if (rx.UserId != userId)
            throw new ForbiddenException("Don thuoc khong thuoc ve ban");
        if (rx.Status == "cancelled")
            throw new ConflictException("Don thuoc da bi huy - khong the upload file");

        // 3. Resolve paths. Dung IHostEnvironment (Core hosting abstraction) thay vi
        // IWebHostEnvironment de Infrastructure khong phai ref ASP.NET.
        // Webroot mac dinh = ContentRootPath/wwwroot (khop convention ASP.NET).
        var webRoot = Path.Combine(_env.ContentRootPath, "wwwroot");

        var dateFolder = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var newName = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";

        // Relative path luu DB (forward slash de FE ghep URL khong cau hinh OS)
        var relativePath = $"/uploads/prescriptions/{userId}/{dateFolder}/{newName}";

        // Physical path tren disk
        var folder = Path.Combine(webRoot, "uploads", "prescriptions",
            userId.ToString(), dateFolder);
        Directory.CreateDirectory(folder);
        var physicalPath = Path.Combine(folder, newName);

        // 4. Ghi file truoc, neu SaveChanges fail thi cleanup
        await using (var fs = File.Create(physicalPath))
        {
            await content.CopyToAsync(fs, ct);
        }

        try
        {
            var doc = new PrescriptionDocument
            {
                PrescriptionId = rx.Id,
                FileUrl = relativePath,
                VerificationStatus = "pending",
                Notes = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.PrescriptionDocuments.Add(doc);

            // Cap nhat prescription.VerificationStatus theo rule:
            //  - not_required / rejected -> pending (user upload lai)
            //  - pending -> giu pending
            //  - verified -> giu verified (document them la phu)
            if (rx.VerificationStatus is "not_required" or "rejected")
            {
                rx.VerificationStatus = "pending";
                rx.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync(ct);
            return ToDocumentDto(doc);
        }
        catch
        {
            // Rollback file da ghi
            try { if (File.Exists(physicalPath)) File.Delete(physicalPath); }
            catch { /* swallow - file orphan se duoc cleanup script don sau */ }
            throw;
        }
    }

    public async Task<IReadOnlyList<PrescriptionDocumentDto>> ListDocumentsAsync(
        long userId, long prescriptionId, CancellationToken ct = default)
    {
        var rx = await _db.Prescriptions.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == prescriptionId, ct)
            ?? throw new NotFoundException("don thuoc", prescriptionId);
        if (rx.UserId != userId)
            throw new ForbiddenException("Don thuoc khong thuoc ve ban");

        var docs = await _db.PrescriptionDocuments.AsNoTracking()
            .Where(d => d.PrescriptionId == prescriptionId)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new PrescriptionDocumentDto
            {
                Id = d.Id,
                PrescriptionId = d.PrescriptionId,
                FileUrl = d.FileUrl,
                VerificationStatus = d.VerificationStatus,
                VerifiedByPharmacistId = d.VerifiedByPharmacistId,
                Notes = d.Notes,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            })
            .ToListAsync(ct);

        return docs;
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

    private static PrescriptionDocumentDto ToDocumentDto(PrescriptionDocument d) => new()
    {
        Id = d.Id,
        PrescriptionId = d.PrescriptionId,
        FileUrl = d.FileUrl,
        VerificationStatus = d.VerificationStatus,
        VerifiedByPharmacistId = d.VerifiedByPharmacistId,
        Notes = d.Notes,
        CreatedAt = d.CreatedAt,
        UpdatedAt = d.UpdatedAt
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
