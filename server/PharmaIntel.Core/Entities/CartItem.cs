// =============================================================================
// Entity: CartItem (San pham trong gio hang)
// Chuc nang: Luu thuoc nguoi dung da them vao gio hang.
// Quan he: N:1 voi User | N:1 voi Medication.
// Rang buoc: Unique (user_id + medication_id), quantity > 0.
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class CartItem
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long MedicationId { get; set; }
    public int Quantity { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Medication Medication { get; set; } = null!;
}
