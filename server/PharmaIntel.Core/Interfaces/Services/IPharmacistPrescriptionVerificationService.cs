using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.DTOs.Pharmacists;

namespace PharmaIntel.Core.Interfaces.Services;

public interface IPharmacistPrescriptionVerificationService
{
    Task<PagedResult<PrescriptionDocumentVerificationDto>> ListPendingAsync(
        long pharmacistUserId,
        PendingPrescriptionDocumentQuery query,
        CancellationToken ct = default);

    Task<PrescriptionDocumentVerificationDto> VerifyAsync(
        long pharmacistUserId,
        long documentId,
        PrescriptionDocumentDecisionRequest request,
        CancellationToken ct = default);

    Task<PrescriptionDocumentVerificationDto> RejectAsync(
        long pharmacistUserId,
        long documentId,
        PrescriptionDocumentDecisionRequest request,
        CancellationToken ct = default);
}