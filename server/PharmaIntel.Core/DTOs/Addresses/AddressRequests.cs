// =============================================================================
// DTOs: AddressCreateRequest, AddressUpdateRequest, AddressListQuery
// Chuc nang: Input tu client cho cac thao tac CRUD dia chi.
// Validation: o Validators/Addresses/*.
// =============================================================================
using PharmaIntel.Core.DTOs.Common;

namespace PharmaIntel.Core.DTOs.Addresses;

public class AddressCreateRequest
{
    public string RecipientName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Ward { get; set; } = string.Empty;
    public string StreetAddress { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}

public class AddressUpdateRequest
{
    public string RecipientName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Ward { get; set; } = string.Empty;
    public string StreetAddress { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class AddressListQuery : PagedQuery
{
    public string? Q { get; set; }            // tim theo RecipientName / StreetAddress
    public bool? IsActive { get; set; }
}
