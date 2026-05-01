// =============================================================================
// Interface: IPrescriptionService
// Chuc nang: Quan ly don thuoc + items cua user dang dang nhap (user-scoped).
// Quy tac: Items chi sua/them/xoa khi prescription dang `draft`. Status transition
// han che (xem PrescriptionService).
// =============================================================================
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.DTOs.Prescriptions;

namespace PharmaIntel.Core.Interfaces.Services;

public interface IPrescriptionService
{
    Task<PagedResult<PrescriptionListItemDto>> ListMyAsync(long userId, PrescriptionListQuery query, CancellationToken ct = default);
    Task<PrescriptionDto> GetByIdAsync(long userId, long id, CancellationToken ct = default);
    Task<PrescriptionDto> CreateAsync(long userId, PrescriptionCreateRequest request, CancellationToken ct = default);
    Task<PrescriptionDto> UpdateAsync(long userId, long id, PrescriptionUpdateRequest request, CancellationToken ct = default);
    Task DeleteAsync(long userId, long id, CancellationToken ct = default);

    Task<PrescriptionItemDto> AddItemAsync(long userId, long prescriptionId, PrescriptionItemCreateRequest request, CancellationToken ct = default);
    Task<PrescriptionItemDto> UpdateItemAsync(long userId, long prescriptionId, long itemId, PrescriptionItemUpdateRequest request, CancellationToken ct = default);
    Task RemoveItemAsync(long userId, long prescriptionId, long itemId, CancellationToken ct = default);
}
