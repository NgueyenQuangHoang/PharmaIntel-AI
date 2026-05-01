// =============================================================================
// Entity: Address (Dia chi giao hang)
// Chuc nang: Luu dia chi giao hang cua nguoi dung (tinh, quan, phuong, duong).
// Quan he: N:1 voi User | 1:N voi Order.
// Luu y: Moi user chi co 1 dia chi mac dinh (is_default). Dung is_active de soft delete.
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class Address
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Ward { get; set; } = string.Empty;
    public string StreetAddress { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
