// =============================================================================
// DTO: AddressDto
// Chuc nang: Tra ve thong tin dia chi giao hang cho client.
// FullAddress: ghep san "{Street}, {Ward}, {District}, {Province}" cho FE tien hien thi.
// =============================================================================
namespace PharmaIntel.Core.DTOs.Addresses;

public class AddressDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Ward { get; set; } = string.Empty;
    public string StreetAddress { get; set; } = string.Empty;
    public string FullAddress { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
