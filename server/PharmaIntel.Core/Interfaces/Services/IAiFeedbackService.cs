// =============================================================================
// Interface: IAiFeedbackService
// Chuc nang: User tao feedback, admin list/review feedback (Phase 4).
// =============================================================================
using PharmaIntel.Core.DTOs.AiFeedback;

namespace PharmaIntel.Core.Interfaces.Services;

public interface IAiFeedbackService
{
    Task<AiFeedbackDto> CreateAsync(
        long userId,
        CreateAiFeedbackRequest request,
        CancellationToken ct = default);

    Task<IReadOnlyList<AiFeedbackDto>> ListForAdminAsync(
        bool? isReviewed = null,
        string? rating = null,
        CancellationToken ct = default);

    Task<AiFeedbackDto> MarkReviewedAsync(
        long feedbackId,
        ReviewAiFeedbackRequest request,
        CancellationToken ct = default);
}
