// =============================================================================
// Interface: ICategoryService
// Chuc nang: Hop dong CRUD danh muc thuoc + xem cay danh muc.
// =============================================================================
using PharmaIntel.Core.DTOs.Categories;
using PharmaIntel.Core.DTOs.Common;

namespace PharmaIntel.Core.Interfaces.Services;

public interface ICategoryService
{
    Task<PagedResult<CategoryDto>> ListAsync(CategoryListQuery query, CancellationToken ct = default);
    Task<List<CategoryTreeNode>> GetTreeAsync(bool includeInactive, CancellationToken ct = default);
    Task<CategoryDto> GetByIdAsync(long id, CancellationToken ct = default);
    Task<CategoryDto> CreateAsync(CategoryCreateRequest request, CancellationToken ct = default);
    Task<CategoryDto> UpdateAsync(long id, CategoryUpdateRequest request, CancellationToken ct = default);
    Task DeleteAsync(long id, CancellationToken ct = default);
}
