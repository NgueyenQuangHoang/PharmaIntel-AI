// =============================================================================
// DTOs: Stats cho trang admin dashboard.
// - AdminStatsOverviewDto: card tong quan (so user, don, doanh thu, thuoc, hom nay).
// - RevenuePointDto: 1 diem trong line chart doanh thu theo ngay.
// - TopMedicationDto: 1 dong trong bang top san pham ban chay.
// - OrdersByStatusDto: count theo trang thai cho pie chart.
// =============================================================================
namespace PharmaIntel.Core.DTOs.Admin;

public class AdminStatsOverviewDto
{
    public int TotalUsers { get; set; }
    public int TotalAdmins { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalMedications { get; set; }
    public int TotalCategories { get; set; }
    public int OrdersToday { get; set; }
    public decimal RevenueToday { get; set; }
    public int OrdersPending { get; set; }
}

public class RevenuePointDto
{
    public DateOnly Date { get; set; }
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}

public class TopMedicationDto
{
    public long MedicationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}

public class OrdersByStatusDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}
