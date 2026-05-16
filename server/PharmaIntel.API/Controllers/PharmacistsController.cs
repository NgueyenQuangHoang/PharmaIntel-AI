// =============================================================================
// Controller: PharmacistsController
// Chuc nang: CRUD ho so duoc si tu van + listing cong khai cho trang Consultations.
// Endpoints:
//   GET    /api/pharmacists                list co phan trang + filter (cong khai)
//   GET    /api/pharmacists/{id}           chi tiet (cong khai)
//   POST   /api/pharmacists                tao moi (role=admin)
//   PUT    /api/pharmacists/{id}           cap nhat (role=admin)
//   DELETE /api/pharmacists/{id}           xoa (role=admin)
// =============================================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.DTOs.Pharmacists;
using PharmaIntel.Core.Interfaces.Services;

namespace PharmaIntel.API.Controllers;

[ApiController]
[Route("api/pharmacists")]
public class PharmacistsController : ControllerBase
{
    private readonly IPharmacistService _service;

    public PharmacistsController(IPharmacistService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<PharmacistDto>>> List(
        [FromQuery] PharmacistListQuery query, CancellationToken ct)
        => Ok(await _service.ListAsync(query, ct));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<PharmacistDto>> Get(long id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(id, ct));

    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<ActionResult<PharmacistDto>> Create(
        [FromBody] PharmacistCreateRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id:long}")]
    public async Task<ActionResult<PharmacistDto>> Update(
        long id, [FromBody] PharmacistUpdateRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, ct));

    [Authorize(Roles = "admin")]
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}
