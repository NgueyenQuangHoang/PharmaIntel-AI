// =============================================================================
// DTO: CategoryDto
// Chuc nang: Tra ve thong tin category cho client (flat list).
// =============================================================================
namespace PharmaIntel.Core.DTOs.Categories;

public class CategoryDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public int MedicationCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
