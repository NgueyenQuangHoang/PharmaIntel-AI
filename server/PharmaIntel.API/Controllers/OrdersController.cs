// =============================================================================
// Controller: OrdersController
// Chuc nang: Checkout cart -> Order, list/detail don cua user, cancel (user) /
//            update status (admin).
// User endpoints: yeu cau JWT. Admin endpoints: yeu cau role "admin".
// =============================================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaIntel.API.Extensions;
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.DTOs.Orders;
using PharmaIntel.Core.Interfaces.Services;

namespace PharmaIntel.API.Controllers;

[ApiController]
[Authorize]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _service;

    public OrdersController(IOrderService service)
    {
        _service = service;
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<OrderDto>> Checkout(
        [FromBody] CheckoutRequest request, CancellationToken ct)
    {
        var order = await _service.CheckoutAsync(User.GetUserId(), request, ct);
        return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
    }

    [HttpGet("my")]
    public async Task<ActionResult<PagedResult<OrderListItemDto>>> ListMy(
        [FromQuery] OrderListQuery query, CancellationToken ct)
        => Ok(await _service.ListMyAsync(User.GetUserId(), query, ct));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<OrderDto>> Get(long id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(User.GetUserId(), id, ct));

    // User cancel don cua chinh minh khi don dang `pending`.
    [HttpPost("{id:long}/cancel")]
    public async Task<ActionResult<OrderDto>> Cancel(long id, CancellationToken ct)
        => Ok(await _service.CancelMyOrderAsync(User.GetUserId(), id, ct));

    // Admin: cap nhat status theo state machine (pending -> confirmed -> processing -> shipping -> delivered, ...).
    [Authorize(Roles = "admin")]
    [HttpPut("{id:long}/status")]
    public async Task<ActionResult<OrderDto>> UpdateStatus(
        long id, [FromBody] UpdateOrderStatusRequest request, CancellationToken ct)
        => Ok(await _service.AdminUpdateStatusAsync(id, request, ct));

    // Admin: liet ke tat ca don tu moi user (kem fullName, email).
    [Authorize(Roles = "admin")]
    [HttpGet("admin/all")]
    public async Task<ActionResult<PagedResult<AdminOrderListItemDto>>> AdminListAll(
        [FromQuery] OrderListQuery query, CancellationToken ct)
        => Ok(await _service.AdminListAllAsync(query, ct));
}
