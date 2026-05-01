// =============================================================================
// Interface: IAddressService
// Chuc nang: Hop dong CRUD dia chi giao hang cua user (user-scoped).
// Quy tac: Moi user chi co 1 default; khong xoa cung neu da co order tham chieu.
// =============================================================================
using PharmaIntel.Core.DTOs.Addresses;
using PharmaIntel.Core.DTOs.Common;

namespace PharmaIntel.Core.Interfaces.Services;

public interface IAddressService
{
    Task<PagedResult<AddressDto>> ListMyAsync(long userId, AddressListQuery query, CancellationToken ct = default);
    Task<AddressDto> GetByIdAsync(long userId, long id, CancellationToken ct = default);
    Task<AddressDto> CreateAsync(long userId, AddressCreateRequest request, CancellationToken ct = default);
    Task<AddressDto> UpdateAsync(long userId, long id, AddressUpdateRequest request, CancellationToken ct = default);
    Task DeleteAsync(long userId, long id, CancellationToken ct = default);
    Task<AddressDto> SetDefaultAsync(long userId, long id, CancellationToken ct = default);
}
