// =============================================================================
// Interface: IRagDashboardService
// Chuc nang: Tinh tong hop metric chat luong RAG cho admin dashboard.
// =============================================================================
using PharmaIntel.Core.DTOs.RagDashboard;

namespace PharmaIntel.Core.Interfaces.Services;

public interface IRagDashboardService
{
    Task<RagDashboardDto> GetAsync(CancellationToken ct = default);
}
