using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.DTOs.Pharmacists;
using PharmaIntel.Core.Entities;
using PharmaIntel.Core.Exceptions;
using PharmaIntel.Core.Interfaces.Services;
using PharmaIntel.Infrastructure.Data;

namespace PharmaIntel.Infrastructure.Services;

public class PharmacistPrescriptionVerificationService : IPharmacistPrescriptionVerificationService
{
    // Default times for daily reminders, indexed by doses-per-day. Index 0 unused.
    private static readonly TimeOnly[][] DefaultDailySlots =
    {
        Array.Empty<TimeOnly>(),
        new[] { new TimeOnly(8, 0) },
        new[] { new TimeOnly(8, 0), new TimeOnly(20, 0) },
        new[] { new TimeOnly(8, 0), new TimeOnly(13, 0), new TimeOnly(20, 0) },
        new[] { new TimeOnly(7, 0), new TimeOnly(12, 0), new TimeOnly(17, 0), new TimeOnly(22, 0) }
    };

    private static readonly Regex DosesPerDayRegex = new(
        @"(\d+)\s*l(?:ầ|a)n",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Parse so ngay uong tu duration text. Vd: "5 ngay", "10 ngày" -> 5/10.
    // Khong khop -> null = EndDate mo (uong tiep cho den khi dung thu cong).
    private static readonly Regex DurationDaysRegex = new(
        @"(\d+)\s*ng(?:à|a)y",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly PharmaIntelDbContext _db;

    public PharmacistPrescriptionVerificationService(PharmaIntelDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<PrescriptionDocumentVerificationDto>> ListPendingAsync(
        long pharmacistUserId,
        PendingPrescriptionDocumentQuery q,
        CancellationToken ct = default)
    {
        q.Normalize();

        await GetActivePharmacistIdAsync(pharmacistUserId, ct);

        var query = _db.PrescriptionDocuments
            .AsNoTracking()
            .Include(d => d.Prescription)
                .ThenInclude(p => p.User)
            // Bo qua document thuoc don da huy - tranh duoc si waste time verify don da bo.
            .Where(d => d.VerificationStatus == "pending"
                     && d.Prescription.Status != "cancelled");

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(d => d.CreatedAt)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(d => new PrescriptionDocumentVerificationDto
            {
                Id = d.Id,
                PrescriptionId = d.PrescriptionId,
                UserId = d.Prescription.UserId,
                UserFullName = d.Prescription.User.FullName,
                PrescriptionTitle = d.Prescription.Title,
                FileUrl = d.FileUrl,
                VerificationStatus = d.VerificationStatus,
                VerifiedByPharmacistId = d.VerifiedByPharmacistId,
                Notes = d.Notes,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            })
            .ToListAsync(ct);

        return new PagedResult<PrescriptionDocumentVerificationDto>
        {
            Items = items,
            Page = q.Page,
            PageSize = q.PageSize,
            TotalCount = total
        };
    }

    public async Task<PagedResult<PrescriptionDocumentVerificationDto>> ListHistoryAsync(
        long pharmacistUserId,
        PrescriptionDocumentHistoryQuery q,
        CancellationToken ct = default)
    {
        q.Normalize();
        await GetActivePharmacistIdAsync(pharmacistUserId, ct);

        var query = _db.PrescriptionDocuments
            .AsNoTracking()
            .Include(d => d.Prescription)
                .ThenInclude(p => p.User)
            .Where(d => d.VerificationStatus == "verified" || d.VerificationStatus == "rejected");

        if (q.Status is "verified" or "rejected")
            query = query.Where(d => d.VerificationStatus == q.Status);

        var total = await query.CountAsync(ct);

        var items = await query
            // Sap xep theo thoi diem ra quyet dinh moi nhat (UpdatedAt) - du cho duoc si khac
            // co the verify nhieu don cu, cai moi nhat luon o tren.
            .OrderByDescending(d => d.UpdatedAt)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(d => new PrescriptionDocumentVerificationDto
            {
                Id = d.Id,
                PrescriptionId = d.PrescriptionId,
                UserId = d.Prescription.UserId,
                UserFullName = d.Prescription.User.FullName,
                PrescriptionTitle = d.Prescription.Title,
                FileUrl = d.FileUrl,
                VerificationStatus = d.VerificationStatus,
                VerifiedByPharmacistId = d.VerifiedByPharmacistId,
                Notes = d.Notes,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            })
            .ToListAsync(ct);

        return new PagedResult<PrescriptionDocumentVerificationDto>
        {
            Items = items,
            Page = q.Page,
            PageSize = q.PageSize,
            TotalCount = total
        };
    }

    public async Task<PrescriptionDocumentVerificationDto> VerifyAsync(
        long pharmacistUserId,
        long documentId,
        PrescriptionDocumentDecisionRequest request,
        CancellationToken ct = default)
    {
        return await DecideAsync(pharmacistUserId, documentId, "verified", request, ct);
    }

    public async Task<PrescriptionDocumentVerificationDto> RejectAsync(
        long pharmacistUserId,
        long documentId,
        PrescriptionDocumentDecisionRequest request,
        CancellationToken ct = default)
    {
        return await DecideAsync(pharmacistUserId, documentId, "rejected", request, ct);
    }

    private async Task<PrescriptionDocumentVerificationDto> DecideAsync(
        long pharmacistUserId,
        long documentId,
        string decision,
        PrescriptionDocumentDecisionRequest request,
        CancellationToken ct)
    {
        // Reject phai co ly do >= 5 ky tu de user biet sua gi. Verify thi notes optional.
        var trimmedNotes = request.Notes?.Trim();
        if (decision == "rejected" && (trimmedNotes is null || trimmedNotes.Length < 5))
            throw new ValidationException("notes", "Phai nhap ly do tu choi (toi thieu 5 ky tu)");

        var pharmacistId = await GetActivePharmacistIdAsync(pharmacistUserId, ct);

        var document = await _db.PrescriptionDocuments
            .Include(d => d.Prescription)
                .ThenInclude(p => p.User)
            .Include(d => d.Prescription)
                .ThenInclude(p => p.Items)
            .FirstOrDefaultAsync(d => d.Id == documentId, ct)
            ?? throw new NotFoundException("file don thuoc", documentId);

        if (document.VerificationStatus != "pending")
            throw new ConflictException($"File don thuoc da o trang thai '{document.VerificationStatus}'");

        if (document.Prescription.Status == "cancelled")
            throw new ConflictException("Don thuoc da bi huy - khong the verify/reject");

        // Verify bat buoc co items (do duoc si nhap truoc) - khong cho verify don rong
        // vi lich nhac uong se khong tao duoc + co the gay nham lan cho user/checkout.
        if (decision == "verified" && document.Prescription.Items.Count == 0)
            throw new ConflictException("Phai nhap danh sach thuoc trong don truoc khi xac minh");

        document.VerificationStatus = decision;
        document.VerifiedByPharmacistId = pharmacistId;
        document.Notes = trimmedNotes;
        document.UpdatedAt = DateTime.UtcNow;

        var prescription = document.Prescription;

        // Chi update prescription.VerificationStatus khi chua duoc quyet (pending/not_required).
        // Tranh trach nhiem document moi reject ghi de prescription da verified truoc do.
        var prescriptionTransitioningToVerified =
            decision == "verified" && prescription.VerificationStatus is "pending" or "not_required";

        if (prescription.VerificationStatus is "pending" or "not_required")
        {
            prescription.VerificationStatus = decision;
            prescription.UpdatedAt = DateTime.UtcNow;
        }

        // Tao MedicationReminder cho tung PrescriptionItem khi prescription chuyen sang verified
        // lan dau. Mac dinh slot uong theo so lan/ngay parse tu item.Frequency.
        if (prescriptionTransitioningToVerified)
            await CreateRemindersForPrescriptionAsync(prescription, ct);

        await _db.SaveChangesAsync(ct);

        return ToDto(document);
    }

    private async Task CreateRemindersForPrescriptionAsync(Prescription prescription, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Skip-if-exists: query (item_id, reminder_time) da co. Bo sung cho unique index
        // o DB - tranh nem UniqueViolation khi verify retry/race. Index van la guarantee
        // cuoi cung neu race chen vao giua check va save.
        var itemIds = prescription.Items.Select(i => i.Id).ToList();
        var existingKeys = await _db.MedicationReminders
            .Where(r => r.PrescriptionItemId != null && itemIds.Contains(r.PrescriptionItemId!.Value))
            .Select(r => new { ItemId = r.PrescriptionItemId!.Value, r.ReminderTime })
            .ToListAsync(ct);
        var existingSet = existingKeys.Select(k => (k.ItemId, k.ReminderTime)).ToHashSet();

        foreach (var item in prescription.Items)
        {
            var slots = ParseDailySlots(item.Frequency);
            var endDate = ParseDurationDays(item.Duration) is int n
                ? today.AddDays(n - 1)
                : (DateOnly?)null;

            foreach (var time in slots)
            {
                if (existingSet.Contains((item.Id, time)))
                    continue;

                _db.MedicationReminders.Add(new MedicationReminder
                {
                    UserId = prescription.UserId,
                    PrescriptionItemId = item.Id,
                    MedicationName = item.MedicationName,
                    FrequencyType = "daily",
                    ReminderTime = time,
                    StartDate = today,
                    EndDate = endDate,
                    Status = "active",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }
    }

    private static int? ParseDurationDays(string? duration)
    {
        if (!string.IsNullOrWhiteSpace(duration))
        {
            var match = DurationDaysRegex.Match(duration);
            if (match.Success && int.TryParse(match.Groups[1].Value, out var n) && n >= 1)
                return n;
        }
        return null;
    }

    private static IReadOnlyList<TimeOnly> ParseDailySlots(string? frequency)
    {
        if (!string.IsNullOrWhiteSpace(frequency))
        {
            var match = DosesPerDayRegex.Match(frequency);
            if (match.Success && int.TryParse(match.Groups[1].Value, out var doses) && doses >= 1)
            {
                var clamped = Math.Min(doses, DefaultDailySlots.Length - 1);
                return DefaultDailySlots[clamped];
            }
        }
        return DefaultDailySlots[1];
    }

    private async Task<long> GetActivePharmacistIdAsync(long pharmacistUserId, CancellationToken ct)
    {
        var pharmacist = await _db.Pharmacists
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == pharmacistUserId, ct)
            ?? throw new ForbiddenException("Tai khoan nay chua gan ho so duoc si");

        if (!pharmacist.IsActive)
            throw new ForbiddenException("Ho so duoc si dang bi vo hieu hoa");

        return pharmacist.Id;
    }

    private static PrescriptionDocumentVerificationDto ToDto(PrescriptionDocument d) => new()
    {
        Id = d.Id,
        PrescriptionId = d.PrescriptionId,
        UserId = d.Prescription.UserId,
        UserFullName = d.Prescription.User.FullName,
        PrescriptionTitle = d.Prescription.Title,
        FileUrl = d.FileUrl,
        VerificationStatus = d.VerificationStatus,
        VerifiedByPharmacistId = d.VerifiedByPharmacistId,
        Notes = d.Notes,
        CreatedAt = d.CreatedAt,
        UpdatedAt = d.UpdatedAt
    };
}