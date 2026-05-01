// =============================================================================
// Interface: IMedicationService
// =============================================================================
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.DTOs.Medications;

namespace PharmaIntel.Core.Interfaces.Services;

public interface IMedicationService
{
    Task<PagedResult<MedicationListItemDto>> ListAsync(MedicationListQuery query, CancellationToken ct = default);
    Task<MedicationDto> GetByIdAsync(long id, CancellationToken ct = default);
    Task<MedicationDto> CreateAsync(MedicationCreateRequest request, CancellationToken ct = default);
    Task<MedicationDto> UpdateAsync(long id, MedicationUpdateRequest request, CancellationToken ct = default);
    Task DeleteAsync(long id, CancellationToken ct = default);
}
