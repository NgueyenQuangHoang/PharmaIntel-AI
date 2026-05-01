// =============================================================================
// Controller: MedicationRemindersController
// Chuc nang: CRUD lich nhac thuoc + log lan nhac cua user dang dang nhap.
// Endpoints (tat ca yeu cau JWT):
//   GET    /api/medication-reminders              list (paged + filter status/q)
//   GET    /api/medication-reminders/{id}         chi tiet
//   POST   /api/medication-reminders              tao moi
//   PUT    /api/medication-reminders/{id}         cap nhat (header + status)
//   DELETE /api/medication-reminders/{id}         xoa (cascade logs)
//   POST   /api/medication-reminders/{id}/logs    them log "taken/missed/skipped"
//   GET    /api/medication-reminders/{id}/logs    list log (paged + filter date/status)
// =============================================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaIntel.API.Extensions;
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.DTOs.MedicationReminders;
using PharmaIntel.Core.Interfaces.Services;

namespace PharmaIntel.API.Controllers;

[ApiController]
[Authorize]
[Route("api/medication-reminders")]
public class MedicationRemindersController : ControllerBase
{
    private readonly IMedicationReminderService _service;

    public MedicationRemindersController(IMedicationReminderService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<MedicationReminderListItemDto>>> List(
        [FromQuery] MedicationReminderListQuery query, CancellationToken ct)
        => Ok(await _service.ListMyAsync(User.GetUserId(), query, ct));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<MedicationReminderDto>> Get(long id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(User.GetUserId(), id, ct));

    [HttpPost]
    public async Task<ActionResult<MedicationReminderDto>> Create(
        [FromBody] MedicationReminderCreateRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(User.GetUserId(), request, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<MedicationReminderDto>> Update(
        long id, [FromBody] MedicationReminderUpdateRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(User.GetUserId(), id, request, ct));

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await _service.DeleteAsync(User.GetUserId(), id, ct);
        return NoContent();
    }

    [HttpPost("{id:long}/logs")]
    public async Task<ActionResult<MedicationReminderLogDto>> AddLog(
        long id, [FromBody] MedicationReminderLogCreateRequest request, CancellationToken ct)
    {
        var log = await _service.AddLogAsync(User.GetUserId(), id, request, ct);
        return CreatedAtAction(nameof(ListLogs), new { id }, log);
    }

    [HttpGet("{id:long}/logs")]
    public async Task<ActionResult<PagedResult<MedicationReminderLogDto>>> ListLogs(
        long id, [FromQuery] MedicationReminderLogListQuery query, CancellationToken ct)
        => Ok(await _service.ListLogsAsync(User.GetUserId(), id, query, ct));
}
