// =============================================================================
// Entity: HealthMetric (Chi so suc khoe)
// Chuc nang: Luu chi so suc khoe nguoi dung (huyet ap, nhip tim, nhiet do, can nang...).
// Quan he: N:1 voi User.
// Luu y: Voi huyet ap, value_number = tam thu, value_number_2 = tam truong.
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class HealthMetric
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string MetricType { get; set; } = string.Empty; // blood_pressure, heart_rate, temperature, weight, blood_sugar, oxygen_saturation
    public decimal ValueNumber { get; set; }
    public decimal? ValueNumber2 { get; set; }
    public string? Unit { get; set; }
    public string? Notes { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
