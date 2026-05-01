// =============================================================================
// Entity: Category (Danh muc thuoc)
// Chuc nang: Phan loai thuoc theo nhom, ho tro danh muc cha/con (self-referencing).
// Quan he: 1:N voi Medication | Tu tham chieu Parent/Children.
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class Category
{
    public long Id { get; set; }
    public long? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();
    public ICollection<Medication> Medications { get; set; } = new List<Medication>();
}
