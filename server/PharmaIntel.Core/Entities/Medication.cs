// =============================================================================
// Entity: Medication (Thuoc / Duoc pham)
// Chuc nang: Luu thong tin thuoc (ten, gia, hoat chat, huong dan, ton kho...).
// Quan he: N:1 voi Category | 1:N voi CartItem, OrderItem, PrescriptionItem,
//          DiagnosticResultMedication.
// Luu y: is_prescription_required = true thi can kiem tra don thuoc truoc khi ban.
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class Medication
{
    public long Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string? Manufacturer { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? Description { get; set; }
    public string? Dosage { get; set; }
    public string? Packaging { get; set; }
    public decimal Price { get; set; }
    public decimal DiscountPercent { get; set; }
    public long CategoryId { get; set; }
    public string? UsageInstructions { get; set; }
    public string? Benefits { get; set; }
    public string? ActiveIngredients { get; set; }
    public string? Contraindications { get; set; }
    public string? SideEffects { get; set; }
    public string? StorageInstructions { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsBestSeller { get; set; }
    public bool IsPrescriptionRequired { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Category Category { get; set; } = null!;
    public ICollection<DiagnosticResultMedication> DiagnosticResultMedications { get; set; } = new List<DiagnosticResultMedication>();
    public ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
