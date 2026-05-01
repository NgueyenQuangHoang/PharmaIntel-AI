// =============================================================================
// Controller: CategoriesController
// Chuc nang: CRUD danh muc thuoc + endpoint xem cay danh muc.
// Endpoints:
//   GET    /api/categories            list co phan trang + filter
//   GET    /api/categories/tree       cay danh muc (tat ca cap)
//   GET    /api/categories/{id}       chi tiet
//   POST   /api/categories            tao moi (auth)
//   PUT    /api/categories/{id}       cap nhat (auth)
//   DELETE /api/categories/{id}       xoa (auth) - chi xoa duoc neu khong co con/thuoc
// =============================================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaIntel.Core.DTOs.Categories;
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.Interfaces.Services;

namespace PharmaIntel.API.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _service;

    public CategoriesController(ICategoryService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<CategoryDto>>> List(
        [FromQuery] CategoryListQuery query, CancellationToken ct)
        => Ok(await _service.ListAsync(query, ct));

    [HttpGet("tree")]
    public async Task<ActionResult<List<CategoryTreeNode>>> Tree(
        [FromQuery] bool includeInactive = false, CancellationToken ct = default)
        => Ok(await _service.GetTreeAsync(includeInactive, ct));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<CategoryDto>> Get(long id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(id, ct));

    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(
        [FromBody] CategoryCreateRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id:long}")]
    public async Task<ActionResult<CategoryDto>> Update(
        long id, [FromBody] CategoryUpdateRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, ct));

    [Authorize(Roles = "admin")]
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}
