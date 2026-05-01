// =============================================================================
// DTO: PagedResult<T>
// Chuc nang: Wrapper cho ket qua phan trang - items + tong so + page hien tai.
// =============================================================================
namespace PharmaIntel.Core.DTOs.Common;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    public bool HasNext => Page < TotalPages;
    public bool HasPrevious => Page > 1;
}
