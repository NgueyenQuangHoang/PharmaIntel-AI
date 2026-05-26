// =============================================================================
// Interface: IConsultationService
// Chuc nang: Hop dong dat lich tu van + duyet/tu choi cua duoc si.
// =============================================================================
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.DTOs.Consultations;

namespace PharmaIntel.Core.Interfaces.Services;

public interface IConsultationService
{
    Task<ConsultationDto> CreateAsync(long userId, CreateConsultationRequest request, CancellationToken ct = default);
    Task<PagedResult<ConsultationDto>> ListForUserAsync(long userId, ListConsultationsQuery query, CancellationToken ct = default);
    Task<PagedResult<ConsultationDto>> ListForPharmacistAsync(long pharmacistUserId, ListConsultationsQuery query, CancellationToken ct = default);
    Task<ConsultationDto> UpdateStatusAsync(long pharmacistUserId, long consultationId, UpdateConsultationStatusRequest request, CancellationToken ct = default);
}
