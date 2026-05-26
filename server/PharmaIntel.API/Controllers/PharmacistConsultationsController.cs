// =============================================================================
// Controller: PharmacistConsultationsController
// Chuc nang: Endpoint cho duoc si xem va xu ly cac yeu cau tu van.
// =============================================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaIntel.API.Extensions;
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.DTOs.Consultations;
using PharmaIntel.Core.Interfaces.Services;

namespace PharmaIntel.API.Controllers;

[ApiController]
[Authorize(Roles = "pharmacist")]
[Route("api/pharmacist/consultations")]
public class PharmacistConsultationsController : ControllerBase
{
    private readonly IConsultationService _service;

    public PharmacistConsultationsController(IConsultationService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ConsultationDto>>> List(
        [FromQuery] ListConsultationsQuery query,
        CancellationToken ct)
    {
        return Ok(await _service.ListForPharmacistAsync(User.GetUserId(), query, ct));
    }

    [HttpPut("{id:long}/status")]
    public async Task<ActionResult<ConsultationDto>> UpdateStatus(
        long id,
        [FromBody] UpdateConsultationStatusRequest request,
        CancellationToken ct)
    {
        return Ok(await _service.UpdateStatusAsync(User.GetUserId(), id, request, ct));
    }
}
