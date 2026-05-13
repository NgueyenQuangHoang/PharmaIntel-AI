// =============================================================================
// Controller: AiFeedbackController
// Chuc nang: User gui feedback (thumbs_up/thumbs_down + reason) cho 1 AI response.
// Endpoints:
//   POST /api/ai-feedback -> tao feedback (yeu cau auth)
// =============================================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaIntel.API.Extensions;
using PharmaIntel.Core.DTOs.AiFeedback;
using PharmaIntel.Core.Interfaces.Services;

namespace PharmaIntel.API.Controllers;

[ApiController]
[Route("api/ai-feedback")]
[Authorize]
public class AiFeedbackController : ControllerBase
{
    private readonly IAiFeedbackService _feedback;

    public AiFeedbackController(IAiFeedbackService feedback)
    {
        _feedback = feedback;
    }

    [HttpPost]
    public async Task<ActionResult<AiFeedbackDto>> Create(
        [FromBody] CreateAiFeedbackRequest request,
        CancellationToken ct)
    {
        var result = await _feedback.CreateAsync(User.GetUserId(), request, ct);
        return Ok(result);
    }
}
