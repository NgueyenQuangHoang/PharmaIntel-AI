// =============================================================================
// DTO: AdminUserDto
// Chuc nang: Hien thi thong tin nguoi dung trong trang admin (kem so don,
//            tong chi tieu de phuc vu danh sach va chi tiet).
// Khac UserProfileDto: them totalOrders + totalSpent, khong co AuthProvider.
// =============================================================================
namespace PharmaIntel.Core.DTOs.Admin;

public class AdminUserDto
{
    public long Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Role { get; set; } = "user";
    public bool IsActive { get; set; }
    public string AuthProvider { get; set; } = "local";
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
