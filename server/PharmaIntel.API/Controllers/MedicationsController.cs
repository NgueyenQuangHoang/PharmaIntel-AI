// =============================================================================
// Controller: MedicationsController
// Chuc nang: CRUD thuoc - list paged + filter, detail, create/update/delete (auth).
// =============================================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.DTOs.Medications;
using PharmaIntel.Core.Interfaces.Services;

namespace PharmaIntel.API.Controllers;

[ApiController]
[Route("api/medications")]
public class MedicationsController : ControllerBase
{
    private readonly IMedicationService _service;

    public MedicationsController(IMedicationService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<MedicationListItemDto>>> List(
        [FromQuery] MedicationListQuery query, CancellationToken ct)
        => Ok(await _service.ListAsync(query, ct));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<MedicationDto>> Get(long id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(id, ct));

    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<ActionResult<MedicationDto>> Create(
        [FromBody] MedicationCreateRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id:long}")]
    public async Task<ActionResult<MedicationDto>> Update(
        long id, [FromBody] MedicationUpdateRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, ct));

    [Authorize(Roles = "admin")]
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}
