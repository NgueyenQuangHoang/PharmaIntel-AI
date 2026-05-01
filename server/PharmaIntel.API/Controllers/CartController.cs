// =============================================================================
// Controller: CartController
// Chuc nang: Quan ly gio hang cua user dang dang nhap.
// Tat ca endpoint deu yeu cau JWT (UserId lay tu claim sub).
// Endpoints:
//   GET    /api/cart                       Lay gio hang
//   POST   /api/cart/items                 Them thuoc (cong don neu da co)
//   PUT    /api/cart/items/{medicationId}  Cap nhat so luong tuyet doi
//   DELETE /api/cart/items/{medicationId}  Xoa 1 thuoc khoi gio
//   DELETE /api/cart                       Xoa toan bo gio
// =============================================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaIntel.API.Extensions;
using PharmaIntel.Core.DTOs.Cart;
using PharmaIntel.Core.Interfaces.Services;

namespace PharmaIntel.API.Controllers;

[ApiController]
[Authorize]
[Route("api/cart")]
public class CartController : ControllerBase
{
    private readonly ICartService _service;

    public CartController(ICartService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<CartDto>> Get(CancellationToken ct)
        => Ok(await _service.GetAsync(User.GetUserId(), ct));

    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> AddItem(
        [FromBody] AddCartItemRequest request, CancellationToken ct)
        => Ok(await _service.AddItemAsync(User.GetUserId(), request, ct));

    [HttpPut("items/{medicationId:long}")]
    public async Task<ActionResult<CartDto>> UpdateItem(
        long medicationId, [FromBody] UpdateCartItemRequest request, CancellationToken ct)
        => Ok(await _service.UpdateItemAsync(User.GetUserId(), medicationId, request, ct));

    [HttpDelete("items/{medicationId:long}")]
    public async Task<ActionResult<CartDto>> RemoveItem(
        long medicationId, CancellationToken ct)
        => Ok(await _service.RemoveItemAsync(User.GetUserId(), medicationId, ct));

    [HttpDelete]
    public async Task<IActionResult> Clear(CancellationToken ct)
    {
        await _service.ClearAsync(User.GetUserId(), ct);
        return NoContent();
    }
}
