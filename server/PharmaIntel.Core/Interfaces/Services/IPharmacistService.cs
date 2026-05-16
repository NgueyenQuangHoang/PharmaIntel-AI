// =============================================================================
// Interface: IPharmacistService
// Chuc nang: Hop dong CRUD ho so duoc si tu van + listing cong khai.
// =============================================================================
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.DTOs.Pharmacists;

namespace PharmaIntel.Core.Interfaces.Services;

public interface IPharmacistService
{
    Task<PagedResult<PharmacistDto>> ListAsync(PharmacistListQuery query, CancellationToken ct = default);
    Task<PharmacistDto> GetByIdAsync(long id, CancellationToken ct = default);
    Task<PharmacistDto> CreateAsync(PharmacistCreateRequest request, CancellationToken ct = default);
    Task<PharmacistDto> UpdateAsync(long id, PharmacistUpdateRequest request, CancellationToken ct = default);
    Task DeleteAsync(long id, CancellationToken ct = default);
}
