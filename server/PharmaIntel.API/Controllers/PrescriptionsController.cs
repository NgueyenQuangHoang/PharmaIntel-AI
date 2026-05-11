// =============================================================================
// Controller: PrescriptionsController
// Chuc nang: CRUD don thuoc + items cua user dang dang nhap (user-scoped).
// Endpoints (tat ca yeu cau JWT):
//   GET    /api/prescriptions/my                         list (paged)
//   GET    /api/prescriptions/{id}                       chi tiet kem items
//   POST   /api/prescriptions                            tao moi (status draft)
//   PUT    /api/prescriptions/{id}                       cap nhat header + status
//   DELETE /api/prescriptions/{id}                       xoa (chi khi chua co order)
//   POST   /api/prescriptions/{id}/items                 them muc thuoc (chi khi draft)
//   PUT    /api/prescriptions/{id}/items/{itemId}        sua muc thuoc (chi khi draft)
//   DELETE /api/prescriptions/{id}/items/{itemId}        xoa muc thuoc (chi khi draft)
// =============================================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaIntel.API.Extensions;
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.DTOs.Prescriptions;
using PharmaIntel.Core.Interfaces.Services;

namespace PharmaIntel.API.Controllers;

[ApiController]
[Authorize]
[Route("api/prescriptions")]
public class PrescriptionsController : ControllerBase
{
    private readonly IPrescriptionService _service;

    public PrescriptionsController(IPrescriptionService service)
    {
        _service = service;
    }

    [HttpGet("my")]
    public async Task<ActionResult<PagedResult<PrescriptionListItemDto>>> ListMy(
        [FromQuery] PrescriptionListQuery query, CancellationToken ct)
        => Ok(await _service.ListMyAsync(User.GetUserId(), query, ct));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<PrescriptionDto>> Get(long id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(User.GetUserId(), id, ct));

    [HttpPost]
    public async Task<ActionResult<PrescriptionDto>> Create(
        [FromBody] PrescriptionCreateRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(User.GetUserId(), request, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<PrescriptionDto>> Update(
        long id, [FromBody] PrescriptionUpdateRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(User.GetUserId(), id, request, ct));

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await _service.DeleteAsync(User.GetUserId(), id, ct);
        return NoContent();
    }

    [HttpPost("{id:long}/items")]
    public async Task<ActionResult<PrescriptionItemDto>> AddItem(
        long id, [FromBody] PrescriptionItemCreateRequest request, CancellationToken ct)
    {
        var item = await _service.AddItemAsync(User.GetUserId(), id, request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.PrescriptionId }, item);
    }

    [HttpPut("{id:long}/items/{itemId:long}")]
    public async Task<ActionResult<PrescriptionItemDto>> UpdateItem(
        long id, long itemId, [FromBody] PrescriptionItemUpdateRequest request, CancellationToken ct)
        => Ok(await _service.UpdateItemAsync(User.GetUserId(), id, itemId, request, ct));

    [HttpDelete("{id:long}/items/{itemId:long}")]
    public async Task<IActionResult> RemoveItem(long id, long itemId, CancellationToken ct)
    {
        await _service.RemoveItemAsync(User.GetUserId(), id, itemId, ct);
        return NoContent();
    }

    // -------------------------------------------------------------------------
    // Document upload / list
    // -------------------------------------------------------------------------

    // POST /api/prescriptions/{id}/documents
    // Form field "file" (IFormFile). Tao row prescription_documents status pending
    // va set prescription.verification_status = pending neu chua duoc duyet.
    [HttpPost("{id:long}/documents")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<PrescriptionDocumentDto>> UploadDocument(
        long id, IFormFile file, CancellationToken ct)
    {
        if (file is null)
            return BadRequest(new { error = "Thieu field 'file'" });

        await using var stream = file.OpenReadStream();
        var dto = await _service.UploadDocumentAsync(
            User.GetUserId(), id, stream, file.FileName, file.ContentType, file.Length, ct);

        return CreatedAtAction(nameof(ListDocuments), new { id }, dto);
    }

    [HttpGet("{id:long}/documents")]
    public async Task<ActionResult<IReadOnlyList<PrescriptionDocumentDto>>> ListDocuments(
        long id, CancellationToken ct)
        => Ok(await _service.ListDocumentsAsync(User.GetUserId(), id, ct));
}
