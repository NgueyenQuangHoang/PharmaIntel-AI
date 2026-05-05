// =============================================================================
// Service: CategoryService
// Chuc nang: Nghiep vu CRUD danh muc - flat list, tim kiem co phan trang.
// Quy tac:
//   - Slug duy nhat (UQ_categories_slug). Trung -> ConflictException(409)
//   - Khong xoa cung neu danh muc dang chua Medications -> ConflictException
// =============================================================================
using Microsoft.EntityFrameworkCore;
using PharmaIntel.Core.DTOs.Categories;
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.Entities;
using PharmaIntel.Core.Exceptions;
using PharmaIntel.Core.Interfaces.Services;
using PharmaIntel.Infrastructure.Data;
using PharmaIntel.Infrastructure.Services.Helpers;

namespace PharmaIntel.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly PharmaIntelDbContext _db;

    public CategoryService(PharmaIntelDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<CategoryDto>> ListAsync(CategoryListQuery q, CancellationToken ct = default)
    {
        q.Normalize();

        var query = _db.Categories.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q.Q))
        {
            var k = q.Q.Trim().ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(k) || c.Slug.Contains(k));
        }

        if (q.IsActive.HasValue) query = query.Where(c => c.IsActive == q.IsActive);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Icon = c.Icon,
                DisplayOrder = c.DisplayOrder,
                IsActive = c.IsActive,
                MedicationCount = c.Medications.Count,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync(ct);

        return new PagedResult<CategoryDto>
        {
            Items = items,
            Page = q.Page,
            PageSize = q.PageSize,
            TotalCount = total
        };
    }

    public async Task<CategoryDto> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var dto = await _db.Categories.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Icon = c.Icon,
                DisplayOrder = c.DisplayOrder,
                IsActive = c.IsActive,
                MedicationCount = c.Medications.Count,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .FirstOrDefaultAsync(ct);

        return dto ?? throw new NotFoundException("danh muc", id);
    }

    public async Task<CategoryDto> CreateAsync(CategoryCreateRequest req, CancellationToken ct = default)
    {
        var slug = string.IsNullOrWhiteSpace(req.Slug) ? SlugHelper.ToSlug(req.Name) : req.Slug.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(slug))
            throw new ValidationException("slug", "Khong the sinh slug tu ten - vui long nhap slug thu cong");

        if (await _db.Categories.AnyAsync(c => c.Slug == slug, ct))
            throw new ConflictException($"Slug '{slug}' da ton tai");

        var entity = new Category
        {
            Name = req.Name.Trim(),
            Slug = slug,
            Icon = req.Icon,
            DisplayOrder = req.DisplayOrder,
            IsActive = req.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Categories.Add(entity);
        await _db.SaveChangesAsync(ct);

        return await GetByIdAsync(entity.Id, ct);
    }

    public async Task<CategoryDto> UpdateAsync(long id, CategoryUpdateRequest req, CancellationToken ct = default)
    {
        var entity = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct)
                     ?? throw new NotFoundException("danh muc", id);

        var slug = req.Slug.Trim().ToLowerInvariant();
        if (slug != entity.Slug && await _db.Categories.AnyAsync(c => c.Slug == slug && c.Id != id, ct))
            throw new ConflictException($"Slug '{slug}' da ton tai");

        entity.Name = req.Name.Trim();
        entity.Slug = slug;
        entity.Icon = req.Icon;
        entity.DisplayOrder = req.DisplayOrder;
        entity.IsActive = req.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await _db.Categories
            .Include(c => c.Medications)
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException("danh muc", id);

        if (entity.Medications.Count > 0)
            throw new ConflictException($"Khong the xoa - danh muc dang chua {entity.Medications.Count} thuoc");

        _db.Categories.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }
}
