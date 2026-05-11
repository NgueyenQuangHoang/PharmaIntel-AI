// =============================================================================
// Service: PharmacistPrescriptionItemService
// Chuc nang: Duoc si nhap/sua/xoa PrescriptionItem dua tren file don bac si.
// Quan he: N:1 voi Prescription | N:1 voi Medication (nullable).
// Quy tac:
//   - Chi duoc si active moi duoc thao tac (lookup theo pharmacists.user_id).
//   - Cho phep sua khi prescription.VerificationStatus IN ('pending','rejected','not_required').
//     Khi prescription da 'verified', items chot - tranh sua phia sau lung dieu duong.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using PharmaIntel.Core.DTOs.Prescriptions;
using PharmaIntel.Core.Entities;
using PharmaIntel.Core.Exceptions;
using PharmaIntel.Core.Interfaces.Services;
using PharmaIntel.Infrastructure.Data;

namespace PharmaIntel.Infrastructure.Services;

public class PharmacistPrescriptionItemService : IPharmacistPrescriptionItemService
{
    private static readonly string[] EditableVerificationStatuses = ["not_required", "pending", "rejected"];

    private readonly PharmaIntelDbContext _db;

    public PharmacistPrescriptionItemService(PharmaIntelDbContext db)
    {
        _db = db;
    }

    public async Task<PrescriptionDto> GetPrescriptionAsync(long pharmacistUserId, long prescriptionId, CancellationToken ct = default)
    {
        await EnsureActivePharmacistAsync(pharmacistUserId, ct);

        var rx = await _db.Prescriptions.AsNoTracking()
            .Include(p => p.Items)
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == prescriptionId, ct)
            ?? throw new NotFoundException("don thuoc", prescriptionId);

        var documents = await _db.PrescriptionDocuments.AsNoTracking()
            .Where(d => d.PrescriptionId == prescriptionId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(ct);

        return new PrescriptionDto
        {
            Id = rx.Id,
            UserId = rx.UserId,
            UserFullName = rx.User?.FullName,
            DoctorId = rx.DoctorId,
            DoctorNameSnapshot = rx.DoctorNameSnapshot,
            Title = rx.Title,
            PrescribedDate = rx.PrescribedDate,
            Status = rx.Status,
            VerificationStatus = rx.VerificationStatus,
            ItemCount = rx.Items.Count,
            CreatedAt = rx.CreatedAt,
            UpdatedAt = rx.UpdatedAt,
            Items = rx.Items.Select(ToItemDto).ToList(),
            Documents = documents.Select(d => new PrescriptionDocumentDto
            {
                Id = d.Id,
                PrescriptionId = d.PrescriptionId,
                FileUrl = d.FileUrl,
                VerificationStatus = d.VerificationStatus,
                VerifiedByPharmacistId = d.VerifiedByPharmacistId,
                Notes = d.Notes,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            }).ToList()
        };
    }

    public async Task<PrescriptionItemDto> AddItemAsync(
        long pharmacistUserId, long prescriptionId, PrescriptionItemCreateRequest req, CancellationToken ct = default)
    {
        await EnsureActivePharmacistAsync(pharmacistUserId, ct);
        var rx = await GetEditablePrescriptionAsync(prescriptionId, ct);

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

    public async Task<PrescriptionItemDto> UpdateItemAsync(
        long pharmacistUserId, long itemId, PrescriptionItemUpdateRequest req, CancellationToken ct = default)
    {
        await EnsureActivePharmacistAsync(pharmacistUserId, ct);

        var item = await _db.PrescriptionItems
            .Include(i => i.Prescription)
            .FirstOrDefaultAsync(i => i.Id == itemId, ct)
            ?? throw new NotFoundException("muc don thuoc", itemId);

        EnsurePrescriptionEditable(item.Prescription);

        var (medicationId, medicationName) = await ResolveMedicationAsync(req.MedicationId, req.MedicationName, ct);

        item.MedicationId = medicationId;
        item.MedicationName = medicationName;
        item.Dosage = req.Dosage?.Trim();
        item.Frequency = req.Frequency?.Trim();
        item.Duration = req.Duration?.Trim();
        item.Prescription.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return ToItemDto(item);
    }

    public async Task RemoveItemAsync(long pharmacistUserId, long itemId, CancellationToken ct = default)
    {
        await EnsureActivePharmacistAsync(pharmacistUserId, ct);

        var item = await _db.PrescriptionItems
            .Include(i => i.Prescription)
            .FirstOrDefaultAsync(i => i.Id == itemId, ct)
            ?? throw new NotFoundException("muc don thuoc", itemId);

        EnsurePrescriptionEditable(item.Prescription);

        _db.PrescriptionItems.Remove(item);
        item.Prescription.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private async Task EnsureActivePharmacistAsync(long pharmacistUserId, CancellationToken ct)
    {
        var pharmacist = await _db.Pharmacists.AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == pharmacistUserId, ct)
            ?? throw new ForbiddenException("Tai khoan nay chua gan ho so duoc si");

        if (!pharmacist.IsActive)
            throw new ForbiddenException("Ho so duoc si dang bi vo hieu hoa");
    }

    private async Task<Prescription> GetEditablePrescriptionAsync(long prescriptionId, CancellationToken ct)
    {
        var rx = await _db.Prescriptions.FirstOrDefaultAsync(p => p.Id == prescriptionId, ct)
                 ?? throw new NotFoundException("don thuoc", prescriptionId);
        EnsurePrescriptionEditable(rx);
        return rx;
    }

    private static void EnsurePrescriptionEditable(Prescription rx)
    {
        if (!EditableVerificationStatuses.Contains(rx.VerificationStatus))
            throw new ConflictException(
                $"Khong the chinh sua items khi don thuoc dang o trang thai verify '{rx.VerificationStatus}'");
        if (rx.Status == "cancelled")
            throw new ConflictException("Don thuoc da bi huy - khong the chinh sua items");
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
            var name = string.IsNullOrWhiteSpace(medicationName) ? med.Name : medicationName.Trim();
            return (med.Id, name);
        }

        var trimmed = medicationName?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(trimmed))
            throw new ValidationException("medicationName", "MedicationName la bat buoc khi khong chon thuoc tu danh muc");
        return (null, trimmed);
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
}
