// =============================================================================
// Interface: ICartService
// =============================================================================
using PharmaIntel.Core.DTOs.Cart;

namespace PharmaIntel.Core.Interfaces.Services;

public interface ICartService
{
    Task<CartDto> GetAsync(long userId, CancellationToken ct = default);
    Task<CartDto> AddItemAsync(long userId, AddCartItemRequest request, CancellationToken ct = default);
    Task<CartDto> UpdateItemAsync(long userId, long medicationId, UpdateCartItemRequest request, CancellationToken ct = default);
    Task<CartDto> RemoveItemAsync(long userId, long medicationId, CancellationToken ct = default);
    Task ClearAsync(long userId, CancellationToken ct = default);
}
