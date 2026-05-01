// =============================================================================
// Controller: AddressesController
// Chuc nang: CRUD dia chi giao hang cua user dang dang nhap.
// Endpoints (deu yeu cau auth - dia chi la user-scoped):
//   GET    /api/addresses/my            list dia chi cua minh (paged)
//   GET    /api/addresses/{id}          chi tiet (chi owner)
//   POST   /api/addresses               tao moi
//   PUT    /api/addresses/{id}          cap nhat
//   DELETE /api/addresses/{id}          xoa (soft delete neu da co order tham chieu)
//   PUT    /api/addresses/{id}/default  dat lam mac dinh
// =============================================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaIntel.API.Extensions;
using PharmaIntel.Core.DTOs.Addresses;
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.Interfaces.Services;

namespace PharmaIntel.API.Controllers;

[ApiController]
[Authorize]
[Route("api/addresses")]
public class AddressesController : ControllerBase
{
    private readonly IAddressService _service;

    public AddressesController(IAddressService service)
    {
        _service = service;
    }

    [HttpGet("my")]
    public async Task<ActionResult<PagedResult<AddressDto>>> ListMy(
        [FromQuery] AddressListQuery query, CancellationToken ct)
        => Ok(await _service.ListMyAsync(User.GetUserId(), query, ct));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<AddressDto>> Get(long id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(User.GetUserId(), id, ct));

    [HttpPost]
    public async Task<ActionResult<AddressDto>> Create(
        [FromBody] AddressCreateRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(User.GetUserId(), request, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<AddressDto>> Update(
        long id, [FromBody] AddressUpdateRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(User.GetUserId(), id, request, ct));

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await _service.DeleteAsync(User.GetUserId(), id, ct);
        return NoContent();
    }

    [HttpPut("{id:long}/default")]
    public async Task<ActionResult<AddressDto>> SetDefault(long id, CancellationToken ct)
        => Ok(await _service.SetDefaultAsync(User.GetUserId(), id, ct));
}
