// =============================================================================
// DTO: PagedQuery (base)
// Chuc nang: Tham so chung cho cac query phan trang - page (1-based), pageSize.
// Kem giang vien-rep: Normalize() de chuan hoa gia tri (toi thieu 1, max 100).
// =============================================================================
namespace PharmaIntel.Core.DTOs.Common;

public abstract class PagedQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public void Normalize(int maxPageSize = 100)
    {
        if (Page < 1) Page = 1;
        if (PageSize < 1) PageSize = 20;
        if (PageSize > maxPageSize) PageSize = maxPageSize;
    }
}
