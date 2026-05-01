// =============================================================================
// DTOs: CartDto, CartItemDto
// Chuc nang: Tra ve cart cua user kem thong tin thuoc va tong tien tinh san.
// FE chi can render.
// =============================================================================
namespace PharmaIntel.Core.DTOs.Cart;

public class CartItemDto
{
    public long Id { get; set; }
    public long MedicationId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Manufacturer { get; set; }
    public decimal UnitPrice { get; set; }            // gia goc
    public decimal DiscountPercent { get; set; }
    public decimal FinalUnitPrice => Math.Round(UnitPrice * (1 - DiscountPercent / 100m), 2);
    public int Quantity { get; set; }
    public decimal LineTotal => Math.Round(FinalUnitPrice * Quantity, 2);
    public bool IsPrescriptionRequired { get; set; }
    public int StockQuantity { get; set; }
    public bool IsAvailable { get; set; }             // false neu inactive hoac het hang
    public DateTime AddedAt { get; set; }
}

public class CartDto
{
    public List<CartItemDto> Items { get; set; } = [];
    public int TotalItems => Items.Sum(i => i.Quantity);
    public int DistinctItems => Items.Count;
    public decimal Subtotal => Items.Sum(i => i.UnitPrice * i.Quantity);
    public decimal TotalDiscount => Items.Sum(i => (i.UnitPrice - i.FinalUnitPrice) * i.Quantity);
    public decimal Total => Items.Sum(i => i.LineTotal);
    public bool HasUnavailableItems => Items.Any(i => !i.IsAvailable);
    public bool HasPrescriptionRequired => Items.Any(i => i.IsPrescriptionRequired);
}
