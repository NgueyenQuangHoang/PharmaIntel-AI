// =============================================================================
// Service: PharmacistService
// Chuc nang: CRUD ho so duoc si hien thi tren trang Tu van truc tuyen.
// Quy tac:
//   - LicenseNumber duy nhat. Neu khong cung cap khi tao -> auto sinh PH-yyyyMMdd-xxxx
//   - Rating 0..5; ExperienceYears, ReviewsCount >= 0 (CHECK constraint o DB).
//   - Khong cho xoa neu ho so dang co PrescriptionDocument duoc xac minh.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.DTOs.Pharmacists;
using PharmaIntel.Core.Entities;
using PharmaIntel.Core.Exceptions;
using PharmaIntel.Core.Interfaces.Services;
using PharmaIntel.Infrastructure.Data;

namespace PharmaIntel.Infrastructure.Services;

public class PharmacistService : IPharmacistService
{
    private readonly PharmaIntelDbContext _db;

    public PharmacistService(PharmaIntelDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<PharmacistDto>> ListAsync(PharmacistListQuery q, CancellationToken ct = default)
    {
        q.Normalize();

        var query = _db.Pharmacists.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q.Q))
        {
            var k = q.Q.Trim().ToLower();
            query = query.Where(p => p.FullName.ToLower().Contains(k));
        }

        if (!string.IsNullOrWhiteSpace(q.Specialization))
            query = query.Where(p => p.Specialization == q.Specialization);

        if (q.IsOnline.HasValue) query = query.Where(p => p.IsOnline == q.IsOnline);
        if (q.IsActive.HasValue) query = query.Where(p => p.IsActive == q.IsActive);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.IsOnline)
            .ThenByDescending(p => p.Rating)
            .ThenBy(p => p.FullName)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(p => MapToDto(p))
            .ToListAsync(ct);

        return new PagedResult<PharmacistDto>
        {
            Items = items,
            Page = q.Page,
            PageSize = q.PageSize,
            TotalCount = total
        };
    }

    public async Task<PharmacistDto> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var dto = await _db.Pharmacists.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => MapToDto(p))
            .FirstOrDefaultAsync(ct);

        return dto ?? throw new NotFoundException("duoc si", id);
    }

    public async Task<PharmacistDto> CreateAsync(PharmacistCreateRequest req, CancellationToken ct = default)
    {
        ValidateRanges(req.Rating, req.ExperienceYears, req.ReviewsCount);

        var license = string.IsNullOrWhiteSpace(req.LicenseNumber)
            ? await GenerateLicenseAsync(ct)
            : req.LicenseNumber.Trim();

        if (await _db.Pharmacists.AnyAsync(p => p.LicenseNumber == license, ct))
            throw new ConflictException($"So giay phep '{license}' da ton tai");

        var entity = new Pharmacist
        {
            FullName = req.FullName.Trim(),
            LicenseNumber = license,
            Specialization = req.Specialization,
            Phone = req.Phone,
            Email = req.Email,
            AvatarUrl = req.AvatarUrl,
            IsOnline = req.IsOnline,
            IsActive = req.IsActive,
            ExperienceYears = req.ExperienceYears,
            About = req.About,
            Rating = req.Rating,
            ReviewsCount = req.ReviewsCount,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Pharmacists.Add(entity);
        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(entity.Id, ct);
    }

    public async Task<PharmacistDto> UpdateAsync(long id, PharmacistUpdateRequest req, CancellationToken ct = default)
    {
        ValidateRanges(req.Rating, req.ExperienceYears, req.ReviewsCount);

        var entity = await _db.Pharmacists.FirstOrDefaultAsync(p => p.Id == id, ct)
                     ?? throw new NotFoundException("duoc si", id);

        var license = req.LicenseNumber.Trim();
        if (license != entity.LicenseNumber &&
            await _db.Pharmacists.AnyAsync(p => p.LicenseNumber == license && p.Id != id, ct))
            throw new ConflictException($"So giay phep '{license}' da ton tai");

        entity.FullName = req.FullName.Trim();
        entity.LicenseNumber = license;
        entity.Specialization = req.Specialization;
        entity.Phone = req.Phone;
        entity.Email = req.Email;
        entity.AvatarUrl = req.AvatarUrl;
        entity.IsOnline = req.IsOnline;
        entity.IsActive = req.IsActive;
        entity.ExperienceYears = req.ExperienceYears;
        entity.About = req.About;
        entity.Rating = req.Rating;
        entity.ReviewsCount = req.ReviewsCount;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await _db.Pharmacists
            .Include(p => p.VerifiedDocuments)
            .Include(p => p.ChatSessions)
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException("duoc si", id);

        if (entity.UserId.HasValue)
            throw new ConflictException("Khong the xoa - ho so dang gan voi tai khoan duoc si. Hay vo hieu hoa (isActive=false) thay vi xoa.");

        if (entity.VerifiedDocuments.Count > 0 || entity.ChatSessions.Count > 0)
            throw new ConflictException("Khong the xoa - ho so co lich su xac minh don thuoc hoac chat. Hay vo hieu hoa thay vi xoa.");

        _db.Pharmacists.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    private static PharmacistDto MapToDto(Pharmacist p) => new()
    {
        Id = p.Id,
        FullName = p.FullName,
        LicenseNumber = p.LicenseNumber,
        Specialization = p.Specialization,
        Phone = p.Phone,
        Email = p.Email,
        AvatarUrl = p.AvatarUrl,
        IsOnline = p.IsOnline,
        IsActive = p.IsActive,
        ExperienceYears = p.ExperienceYears,
        About = p.About,
        Rating = p.Rating,
        ReviewsCount = p.ReviewsCount,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };

    private static void ValidateRanges(decimal rating, int experienceYears, int reviewsCount)
    {
        if (rating < 0m || rating > 5m)
            throw new ValidationException("rating", "Rating phai nam trong khoang 0..5");
        if (experienceYears < 0)
            throw new ValidationException("experienceYears", "So nam kinh nghiem khong duoc am");
        if (reviewsCount < 0)
            throw new ValidationException("reviewsCount", "So luot danh gia khong duoc am");
    }

    private async Task<string> GenerateLicenseAsync(CancellationToken ct)
    {
        var prefix = $"PH-{DateTime.UtcNow:yyyyMMdd}-";
        for (int i = 0; i < 5; i++)
        {
            var candidate = prefix + Random.Shared.Next(1000, 9999);
            if (!await _db.Pharmacists.AnyAsync(p => p.LicenseNumber == candidate, ct))
                return candidate;
        }
        return prefix + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
    }
}
