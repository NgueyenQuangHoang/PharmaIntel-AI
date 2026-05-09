// =============================================================================
// DTOs: OrderDto, OrderListItemDto, OrderItemDto
// Chuc nang: Tra ve don hang cho client.
//   - OrderListItemDto: list view (gon)
//   - OrderDto: detail view (full + items)
// =============================================================================
namespace PharmaIntel.Core.DTOs.Orders;

public class OrderItemDto
{
    public long Id { get; set; }
    public long? MedicationId { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal TotalPrice { get; set; }
}

public class OrderListItemDto
{
    public long Id { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string? PaymentTypeSnapshot { get; set; }
    public int ItemCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OrderDto : OrderListItemDto
{
    public long? AddressId { get; set; }
    public long? PaymentMethodId { get; set; }
    public string? ShippingRecipientName { get; set; }
    public string? ShippingPhone { get; set; }
    public string? ShippingFullAddress { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = [];

    // Chi co gia tri khi PaymentTypeSnapshot == "bank_transfer".
    // BE sinh moi lan GET (khong luu DB) - cau hinh ngan hang co the doi.
    public string? VietQrUrl { get; set; }
    public string? TransferContent { get; set; }
}

// Admin view: them user info de admin biet don nay cua ai
public class AdminOrderListItemDto : OrderListItemDto
{
    public long UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
}
