// =============================================================================
// DTO: SymptomDto
// Chuc nang: Tra ve danh muc trieu chung cho client (de chon luc bat dau chan doan).
// =============================================================================
namespace PharmaIntel.Core.DTOs.Diagnostics;

public class SymptomDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? GroupName { get; set; }
    public int DisplayOrder { get; set; }
}
