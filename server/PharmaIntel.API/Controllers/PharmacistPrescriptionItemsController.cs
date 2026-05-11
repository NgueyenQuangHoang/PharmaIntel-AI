// =============================================================================
// Controller: PharmacistPrescriptionItemsController
// Chuc nang: Duoc si xem chi tiet don thuoc + nhap/sua/xoa items theo file don bac si.
// Endpoints (yeu cau role=pharmacist):
//   GET    /api/pharmacist/prescriptions/{id}                chi tiet don (kem items + documents)
//   POST   /api/pharmacist/prescriptions/{id}/items          them muc thuoc
//   PUT    /api/pharmacist/prescription-items/{id}           sua muc thuoc
//   DELETE /api/pharmacist/prescription-items/{id}           xoa muc thuoc
// =============================================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaIntel.API.Extensions;
using PharmaIntel.Core.DTOs.Prescriptions;
using PharmaIntel.Core.Interfaces.Services;

namespace PharmaIntel.API.Controllers;

[ApiController]
[Authorize(Roles = "pharmacist")]
[Route("api/pharmacist")]
public class PharmacistPrescriptionItemsController : ControllerBase
{
    private readonly IPharmacistPrescriptionItemService _service;

    public PharmacistPrescriptionItemsController(IPharmacistPrescriptionItemService service)
    {
        _service = service;
    }

    [HttpGet("prescriptions/{id:long}")]
    public async Task<ActionResult<PrescriptionDto>> GetPrescription(long id, CancellationToken ct)
        => Ok(await _service.GetPrescriptionAsync(User.GetUserId(), id, ct));

    [HttpPost("prescriptions/{id:long}/items")]
    public async Task<ActionResult<PrescriptionItemDto>> AddItem(
        long id, [FromBody] PrescriptionItemCreateRequest request, CancellationToken ct)
    {
        var item = await _service.AddItemAsync(User.GetUserId(), id, request, ct);
        return CreatedAtAction(nameof(GetPrescription), new { id = item.PrescriptionId }, item);
    }

    [HttpPut("prescription-items/{id:long}")]
    public async Task<ActionResult<PrescriptionItemDto>> UpdateItem(
        long id, [FromBody] PrescriptionItemUpdateRequest request, CancellationToken ct)
        => Ok(await _service.UpdateItemAsync(User.GetUserId(), id, request, ct));

    [HttpDelete("prescription-items/{id:long}")]
    public async Task<IActionResult> RemoveItem(long id, CancellationToken ct)
    {
        await _service.RemoveItemAsync(User.GetUserId(), id, ct);
        return NoContent();
    }
}
