// =============================================================================
// Entity: PaymentMethod (Phuong thuc thanh toan)
// Chuc nang: Luu phuong thuc thanh toan cua user (COD, MoMo, ZaloPay, VNPay, the...).
// Quan he: N:1 voi User | 1:N voi Order, PaymentTransaction.
// Luu y: KHONG luu so the/vi goc - chi luu masked_account va provider token.
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class PaymentMethod
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string PaymentType { get; set; } = string.Empty; // cod, bank_transfer, momo, zalopay, vnpay, credit_card
    public string DisplayName { get; set; } = string.Empty;
    public string? MaskedAccount { get; set; }
    public string? ProviderCustomerId { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<PaymentTransaction> Transactions { get; set; } = new List<PaymentTransaction>();
}
