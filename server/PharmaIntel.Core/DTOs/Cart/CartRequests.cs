// =============================================================================
// DTOs: AddCartItemRequest, UpdateCartItemRequest
// Chuc nang: Input cho them/sua item gio hang.
// =============================================================================
namespace PharmaIntel.Core.DTOs.Cart;

public class AddCartItemRequest
{
    public long MedicationId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class UpdateCartItemRequest
{
    public int Quantity { get; set; }
}
