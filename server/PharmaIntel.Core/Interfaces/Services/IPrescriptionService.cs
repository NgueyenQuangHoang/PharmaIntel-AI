// =============================================================================
// Interface: IPrescriptionService
// Chuc nang: Quan ly don thuoc + upload file (user-scoped).
// Quy tac: User KHONG nhap PrescriptionItems - chi tao don rong + upload anh/PDF
// don bac si. Duoc si la nguoi doc file va nhap items
// (xem IPharmacistPrescriptionItemService).
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

    // Upload file anh/PDF cho prescription. Stream se duoc copy ngay sang disk; caller
    // chiu trach nhiem dispose. Khong dung IFormFile o Core de tranh ref ASP.NET.
    Task<PrescriptionDocumentDto> UploadDocumentAsync(
        long userId,
        long prescriptionId,
        Stream content,
        string fileName,
        string contentType,
        long lengthBytes,
        CancellationToken ct = default);

    Task<IReadOnlyList<PrescriptionDocumentDto>> ListDocumentsAsync(
        long userId, long prescriptionId, CancellationToken ct = default);
}
