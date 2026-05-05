// =============================================================================
// Interface: IOrderService
// =============================================================================
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.DTOs.Orders;

namespace PharmaIntel.Core.Interfaces.Services;

public interface IOrderService
{
    Task<OrderDto> CheckoutAsync(long userId, CheckoutRequest request, CancellationToken ct = default);
    Task<PagedResult<OrderListItemDto>> ListMyAsync(long userId, OrderListQuery query, CancellationToken ct = default);
    Task<OrderDto> GetByIdAsync(long userId, long orderId, CancellationToken ct = default);

    // User: chi cancel duoc don pending cua chinh minh.
    Task<OrderDto> CancelMyOrderAsync(long userId, long orderId, CancellationToken ct = default);

    // Admin: any state machine transition, khong check ownership.
    Task<OrderDto> AdminUpdateStatusAsync(long orderId, UpdateOrderStatusRequest request, CancellationToken ct = default);

    // Admin: liet ke tat ca don tu moi user, kem snapshot user (fullName, email).
    Task<PagedResult<AdminOrderListItemDto>> AdminListAllAsync(OrderListQuery query, CancellationToken ct = default);
}
