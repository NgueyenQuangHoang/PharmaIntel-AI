// =============================================================================
// Entity: OrderItem (Chi tiet don hang)
// Chuc nang: Tung dong san pham trong don hang (ten, so luong, don gia, giam gia).
// Quan he: N:1 voi Order | N:1 voi Medication (nullable).
// Luu y: Luu snapshot medication_name, unit_price, discount_percent tai thoi diem dat.
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class OrderItem
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public long? MedicationId { get; set; }
    public long? PrescriptionItemId { get; set; }
    public string MedicationNameSnapshot { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal TotalPrice { get; set; }

    public Order Order { get; set; } = null!;
    public Medication? Medication { get; set; }
}
