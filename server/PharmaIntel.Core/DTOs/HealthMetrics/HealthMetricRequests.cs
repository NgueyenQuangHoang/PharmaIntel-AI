// =============================================================================
// DTOs: HealthMetricCreateRequest, HealthMetricUpdateRequest, HealthMetricListQuery
// Validation: o Validators/HealthMetrics/*.
// =============================================================================
using PharmaIntel.Core.DTOs.Common;

namespace PharmaIntel.Core.DTOs.HealthMetrics;

public class HealthMetricCreateRequest
{
    public string MetricType { get; set; } = string.Empty;
    public decimal ValueNumber { get; set; }
    public decimal? ValueNumber2 { get; set; }   // bat buoc khi MetricType = blood_pressure
    public string? Unit { get; set; }            // optional - service auto-fill default
    public string? Notes { get; set; }
    public DateTime? RecordedAt { get; set; }    // optional - default UtcNow
}

public class HealthMetricUpdateRequest
{
    public string MetricType { get; set; } = string.Empty;
    public decimal ValueNumber { get; set; }
    public decimal? ValueNumber2 { get; set; }
    public string? Unit { get; set; }
    public string? Notes { get; set; }
    public DateTime? RecordedAt { get; set; }
}

public class HealthMetricListQuery : PagedQuery
{
    public string? MetricType { get; set; }      // filter theo loai
    public DateTime? FromDate { get; set; }      // RecordedAt >= FromDate
    public DateTime? ToDate { get; set; }        // RecordedAt <= ToDate
}
