// =============================================================================
// Controller: AdminAiFeedbackController
// Chuc nang: Admin list + review feedback (Phase 4).
// Endpoints:
//   GET  /api/admin/ai-feedback           list (filter isReviewed, rating)
//   POST /api/admin/ai-feedback/{id}/review mark da review + AdminNote
// =============================================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaIntel.Core.DTOs.AiFeedback;
using PharmaIntel.Core.Interfaces.Services;

namespace PharmaIntel.API.Controllers;

[ApiController]
[Route("api/admin/ai-feedback")]
[Authorize(Roles = "admin")]
public class AdminAiFeedbackController : ControllerBase
{
    private readonly IAiFeedbackService _feedback;

    public AdminAiFeedbackController(IAiFeedbackService feedback)
    {
        _feedback = feedback;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AiFeedbackDto>>> List(
        [FromQuery] bool? isReviewed,
        [FromQuery] string? rating,
        CancellationToken ct)
    {
        var result = await _feedback.ListForAdminAsync(isReviewed, rating, ct);
        return Ok(result);
    }

    [HttpPost("{feedbackId:long}/review")]
    public async Task<ActionResult<AiFeedbackDto>> Review(
        long feedbackId,
        [FromBody] ReviewAiFeedbackRequest request,
        CancellationToken ct)
    {
        var result = await _feedback.MarkReviewedAsync(feedbackId, request, ct);
        return Ok(result);
    }
}
