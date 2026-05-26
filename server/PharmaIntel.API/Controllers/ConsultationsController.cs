// =============================================================================
// Controller: ConsultationsController
// Chuc nang: Endpoint cho nguoi dung dat lich tu van + xem lich cua minh.
// =============================================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaIntel.API.Extensions;
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.DTOs.Consultations;
using PharmaIntel.Core.Interfaces.Services;

namespace PharmaIntel.API.Controllers;

[ApiController]
[Authorize]
[Route("api/consultations")]
public class ConsultationsController : ControllerBase
{
    private readonly IConsultationService _service;

    public ConsultationsController(IConsultationService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<ConsultationDto>> Create(
        [FromBody] CreateConsultationRequest request,
        CancellationToken ct)
    {
        var dto = await _service.CreateAsync(User.GetUserId(), request, ct);
        return CreatedAtAction(nameof(ListMine), new { }, dto);
    }

    [HttpGet("my")]
    public async Task<ActionResult<PagedResult<ConsultationDto>>> ListMine(
        [FromQuery] ListConsultationsQuery query,
        CancellationToken ct)
    {
        return Ok(await _service.ListForUserAsync(User.GetUserId(), query, ct));
    }
}
