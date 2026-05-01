// =============================================================================
// DTOs: MedicationCreateRequest, MedicationUpdateRequest, MedicationListQuery
// =============================================================================
using PharmaIntel.Core.DTOs.Common;

namespace PharmaIntel.Core.DTOs.Medications;

public class MedicationCreateRequest
{
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
}

public class MedicationUpdateRequest : MedicationCreateRequest { }

public class MedicationListQuery : PagedQuery
{
    public string? Q { get; set; }              // tim theo Name / Sku / GenericName
    public long? CategoryId { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsFeatured { get; set; }
    public bool? IsBestSeller { get; set; }
    public bool? IsPrescriptionRequired { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SortBy { get; set; }         // name | price | createdAt
    public bool SortDesc { get; set; }
}
