// =============================================================================
// DTOs: CategoryCreateRequest, CategoryUpdateRequest, CategoryListQuery
// Chuc nang: Input tu client cho cac thao tac CRUD danh muc.
// Validation: o Validators/Categories/*.
// =============================================================================
using PharmaIntel.Core.DTOs.Common;

namespace PharmaIntel.Core.DTOs.Categories;

public class CategoryCreateRequest
{
    public long? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }   // null -> auto generate tu Name
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CategoryUpdateRequest
{
    public long? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

public class CategoryListQuery : PagedQuery
{
    public string? Q { get; set; }            // tim theo Name / Slug
    public long? ParentId { get; set; }
    public bool? IsActive { get; set; }
    public bool RootOnly { get; set; }        // chi lay danh muc cap 1 (parentId == null)
}
