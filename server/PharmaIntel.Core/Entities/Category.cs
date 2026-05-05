// =============================================================================
// Entity: Category (Danh muc thuoc)
// Chuc nang: Phan loai thuoc theo nhom (flat list, khong co cau truc cha/con).
// Quan he: 1:N voi Medication.
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class Category
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Medication> Medications { get; set; } = new List<Medication>();
}
