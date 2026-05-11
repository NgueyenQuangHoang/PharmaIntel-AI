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
            .FirstOrDefaultAsync(d => d.Id == documentId, ct)
            ?? throw new NotFoundException("file don thuoc", documentId);

        if (document.VerificationStatus != "pending")
            throw new ConflictException($"File don thuoc da o trang thai '{document.VerificationStatus}'");

        if (document.Prescription.Status == "cancelled")
            throw new ConflictException("Don thuoc da bi huy - khong the verify/reject");

        document.VerificationStatus = decision;
        document.VerifiedByPharmacistId = pharmacistId;
        document.Notes = trimmedNotes;
        document.UpdatedAt = DateTime.UtcNow;

        var prescription = document.Prescription;

        // Chi update prescription.VerificationStatus khi chua duoc quyet (pending/not_required).
        // Tranh trach nhiem document moi reject ghi de prescription da verified truoc do.
        if (prescription.VerificationStatus is "pending" or "not_required")
        {
            prescription.VerificationStatus = decision;
            prescription.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);

        return ToDto(document);
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