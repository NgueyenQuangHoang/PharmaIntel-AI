// =============================================================================
// Service: CategoryService
// Chuc nang: Nghiep vu CRUD danh muc - tim kiem co phan trang, cay, them/sua/xoa.
// Quy tac:
//   - Slug duy nhat (UQ_categories_slug). Trung -> ConflictException(409)
//   - Khong cho ParentId tro vao chinh no hoac vong lap
//   - Khong xoa cung neu danh muc co Children hoac Medications -> ConflictException
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
        if (q.RootOnly) query = query.Where(c => c.ParentId == null);
        else if (q.ParentId.HasValue) query = query.Where(c => c.ParentId == q.ParentId);

        if (q.IsActive.HasValue) query = query.Where(c => c.IsActive == q.IsActive);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                ParentId = c.ParentId,
                ParentName = c.Parent != null ? c.Parent.Name : null,
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

    public async Task<List<CategoryTreeNode>> GetTreeAsync(bool includeInactive, CancellationToken ct = default)
    {
        var all = await _db.Categories.AsNoTracking()
            .Where(c => includeInactive || c.IsActive)
            .OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name)
            .ToListAsync(ct);

        var lookup = all.ToDictionary(c => c.Id, c => new CategoryTreeNode
        {
            Id = c.Id,
            Name = c.Name,
            Slug = c.Slug,
            Icon = c.Icon,
            DisplayOrder = c.DisplayOrder,
            IsActive = c.IsActive
        });

        var roots = new List<CategoryTreeNode>();
        foreach (var c in all)
        {
            var node = lookup[c.Id];
            if (c.ParentId.HasValue && lookup.TryGetValue(c.ParentId.Value, out var parent))
                parent.Children.Add(node);
            else
                roots.Add(node);
        }
        return roots;
    }

    public async Task<CategoryDto> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var dto = await _db.Categories.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                ParentId = c.ParentId,
                ParentName = c.Parent != null ? c.Parent.Name : null,
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

        if (req.ParentId.HasValue)
        {
            var parentExists = await _db.Categories.AnyAsync(c => c.Id == req.ParentId, ct);
            if (!parentExists) throw new NotFoundException("danh muc cha", req.ParentId);
        }

        var entity = new Category
        {
            ParentId = req.ParentId,
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

        if (req.ParentId.HasValue)
        {
            if (req.ParentId == id)
                throw new ValidationException("parentId", "Danh muc khong the la cha cua chinh no");

            // chong vong lap: parent chain khong duoc chua id hien tai
            var pid = req.ParentId;
            while (pid.HasValue)
            {
                if (pid == id)
                    throw new ValidationException("parentId", "Khong the dat parent gay vong lap");
                pid = await _db.Categories.Where(c => c.Id == pid).Select(c => c.ParentId).FirstOrDefaultAsync(ct);
            }

            var parentExists = await _db.Categories.AnyAsync(c => c.Id == req.ParentId, ct);
            if (!parentExists) throw new NotFoundException("danh muc cha", req.ParentId);
        }

        entity.ParentId = req.ParentId;
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
            .Include(c => c.Children)
            .Include(c => c.Medications)
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException("danh muc", id);

        if (entity.Children.Count > 0)
            throw new ConflictException($"Khong the xoa - danh muc co {entity.Children.Count} danh muc con");
        if (entity.Medications.Count > 0)
            throw new ConflictException($"Khong the xoa - danh muc dang chua {entity.Medications.Count} thuoc");

        _db.Categories.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }
}
