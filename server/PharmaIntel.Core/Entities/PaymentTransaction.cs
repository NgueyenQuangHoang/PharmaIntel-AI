// =============================================================================
// Entity: PaymentTransaction (Giao dich thanh toan)
// Chuc nang: Luu tung giao dich thanh toan (MoMo, ZaloPay, VNPay, COD...).
// Quan he: N:1 voi Order | N:1 voi PaymentMethod (nullable).
// Luu y: Mot don hang co the co nhieu giao dich (that bai roi thanh toan lai, hoan tien).
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class PaymentTransaction
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public long? PaymentMethodId { get; set; }
    public string? Provider { get; set; }
    public string? ProviderTransactionId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "initiated"; // initiated, pending, success, failed, cancelled, refunded
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Order Order { get; set; } = null!;
    public PaymentMethod? PaymentMethod { get; set; }
}
