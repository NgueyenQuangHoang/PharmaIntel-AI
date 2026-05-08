// =============================================================================
// DTOs: CheckoutRequest, UpdateOrderStatusRequest, OrderListQuery
// =============================================================================
using PharmaIntel.Core.DTOs.Common;

namespace PharmaIntel.Core.DTOs.Orders;

public class CheckoutRequest
{
    public long AddressId { get; set; }

    // Null/0 = backend tu dam bao 1 PaymentMethod COD cho user (MVP).
    // Khi tich hop gateway that, FE truyen Id cua method da chon.
    public long? PaymentMethodId { get; set; }
}

public class UpdateOrderStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public class OrderListQuery : PagedQuery
{
    public string? Status { get; set; }
    public string? PaymentStatus { get; set; }
}
