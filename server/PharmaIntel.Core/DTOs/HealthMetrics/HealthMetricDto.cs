// =============================================================================
// DTO: HealthMetricDto
// Chuc nang: Tra ve chi so suc khoe cua user.
// Voi blood_pressure: valueNumber = tam thu, valueNumber2 = tam truong.
// =============================================================================
namespace PharmaIntel.Core.DTOs.HealthMetrics;

public class HealthMetricDto
{
    public long Id { get; set; }
    public string MetricType { get; set; } = string.Empty;
    public decimal ValueNumber { get; set; }
    public decimal? ValueNumber2 { get; set; }
    public string? Unit { get; set; }
    public string? Notes { get; set; }
    public DateTime RecordedAt { get; set; }
}
