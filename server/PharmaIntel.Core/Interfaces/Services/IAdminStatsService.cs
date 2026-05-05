// =============================================================================
// Interface: IAdminStatsService
// Chuc nang: Cung cap so lieu thong ke cho trang admin dashboard.
// Cac endpoint deu read-only, chi cho admin truy cap.
// =============================================================================
using PharmaIntel.Core.DTOs.Admin;

namespace PharmaIntel.Core.Interfaces.Services;

public interface IAdminStatsService
{
    Task<AdminStatsOverviewDto> GetOverviewAsync(CancellationToken ct = default);
    Task<List<RevenuePointDto>> GetRevenueAsync(int days, CancellationToken ct = default);
    Task<List<TopMedicationDto>> GetTopMedicationsAsync(int limit, CancellationToken ct = default);
    Task<List<OrdersByStatusDto>> GetOrdersByStatusAsync(CancellationToken ct = default);
}
