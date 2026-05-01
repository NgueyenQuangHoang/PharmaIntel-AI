// =============================================================================
// DTOs: MedicationDto, MedicationListItemDto
// Chuc nang: Tra ve thong tin thuoc cho client.
// MedicationListItemDto: cho list view (gon hon, khong co cac truong text dai).
// MedicationDto: cho detail view (full).
// =============================================================================
namespace PharmaIntel.Core.DTOs.Medications;

public class MedicationListItemDto
{
    public long Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string? Manufacturer { get; set; }
    public decimal Price { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal FinalPrice => Math.Round(Price * (1 - DiscountPercent / 100m), 2);
    public long CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsBestSeller { get; set; }
    public bool IsPrescriptionRequired { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
}

public class MedicationDto : MedicationListItemDto
{
    public string? RegistrationNumber { get; set; }
    public string? Description { get; set; }
    public string? Dosage { get; set; }
    public string? Packaging { get; set; }
    public string? UsageInstructions { get; set; }
    public string? Benefits { get; set; }
    public string? ActiveIngredients { get; set; }
    public string? Contraindications { get; set; }
    public string? SideEffects { get; set; }
    public string? StorageInstructions { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
