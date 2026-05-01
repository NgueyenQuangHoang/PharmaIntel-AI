// =============================================================================
// Interface: IHealthMetricService
// Chuc nang: Quan ly chi so suc khoe (user-scoped).
// =============================================================================
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.DTOs.HealthMetrics;

namespace PharmaIntel.Core.Interfaces.Services;

public interface IHealthMetricService
{
    Task<PagedResult<HealthMetricDto>> ListMyAsync(long userId, HealthMetricListQuery query, CancellationToken ct = default);
    Task<HealthMetricDto> GetByIdAsync(long userId, long id, CancellationToken ct = default);
    Task<HealthMetricDto> CreateAsync(long userId, HealthMetricCreateRequest request, CancellationToken ct = default);
    Task<HealthMetricDto> UpdateAsync(long userId, long id, HealthMetricUpdateRequest request, CancellationToken ct = default);
    Task DeleteAsync(long userId, long id, CancellationToken ct = default);
}
