// =============================================================================
// Interface: IPharmacistPrescriptionItemService
// Chuc nang: Duoc si nhap/sua/xoa PrescriptionItem theo file don bac si.
// Quy tac:
//   - Chi cho phep thao tac khi prescription.VerificationStatus IN ('pending', 'rejected')
//     - sau khi verified thi don da chot, khong cho doi.
//   - Khong rang buoc theo prescription.Status (user co the dat 'active' nhung duoc si
//     van can sua items neu chua verify).
// =============================================================================
using PharmaIntel.Core.DTOs.Prescriptions;

namespace PharmaIntel.Core.Interfaces.Services;

public interface IPharmacistPrescriptionItemService
{
    Task<PrescriptionDto> GetPrescriptionAsync(long pharmacistUserId, long prescriptionId, CancellationToken ct = default);

    Task<PrescriptionItemDto> AddItemAsync(
        long pharmacistUserId, long prescriptionId, PrescriptionItemCreateRequest request, CancellationToken ct = default);

    Task<PrescriptionItemDto> UpdateItemAsync(
        long pharmacistUserId, long itemId, PrescriptionItemUpdateRequest request, CancellationToken ct = default);

    Task RemoveItemAsync(long pharmacistUserId, long itemId, CancellationToken ct = default);
}
