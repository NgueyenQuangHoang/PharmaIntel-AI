// =============================================================================
// Entity: Order (Don hang)
// Chuc nang: Luu don hang cua nguoi dung (tong tien, trang thai, thong tin giao hang).
// Quan he: N:1 voi User, Address, PaymentMethod, Prescription (tat ca nullable tru User)
//          | 1:N voi OrderItem, PaymentTransaction.
// Luu y: Snapshot dia chi va phuong thuc thanh toan tai thoi diem dat hang
//        (shipping_recipient_name, shipping_full_address, payment_type_snapshot).
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class Order
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long? AddressId { get; set; }
    public long? PaymentMethodId { get; set; }
    public long? PrescriptionId { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal Total { get; set; }
    public string? ShippingRecipientName { get; set; }
    public string? ShippingPhone { get; set; }
    public string? ShippingFullAddress { get; set; }
    public string? PaymentTypeSnapshot { get; set; }
    public string PaymentStatus { get; set; } = "unpaid"; // unpaid, pending, paid, failed, refunded, cod_pending
    public string Status { get; set; } = "pending"; // pending, confirmed, processing, shipping, delivered, cancelled, refunded
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Address? Address { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public Prescription? Prescription { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<PaymentTransaction> Transactions { get; set; } = new List<PaymentTransaction>();
}
