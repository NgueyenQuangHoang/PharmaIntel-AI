// =============================================================================
// DTO: CategoryDto, CategoryTreeNode
// Chuc nang: Tra ve thong tin category cho client.
// =============================================================================
namespace PharmaIntel.Core.DTOs.Categories;

public class CategoryDto
{
    public long Id { get; set; }
    public long? ParentId { get; set; }
    public string? ParentName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public int MedicationCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CategoryTreeNode
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public List<CategoryTreeNode> Children { get; set; } = [];
}
