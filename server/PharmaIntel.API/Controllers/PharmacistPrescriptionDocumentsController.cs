using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaIntel.API.Extensions;
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.DTOs.Pharmacists;
using PharmaIntel.Core.Interfaces.Services;

namespace PharmaIntel.API.Controllers;

[ApiController]
[Authorize(Roles = "pharmacist")]
[Route("api/pharmacist/prescription-documents")]
public class PharmacistPrescriptionDocumentsController : ControllerBase
{
    private readonly IPharmacistPrescriptionVerificationService _service;

    public PharmacistPrescriptionDocumentsController(IPharmacistPrescriptionVerificationService service)
    {
        _service = service;
    }

    [HttpGet("pending")]
    public async Task<ActionResult<PagedResult<PrescriptionDocumentVerificationDto>>> ListPending(
        [FromQuery] PendingPrescriptionDocumentQuery query,
        CancellationToken ct)
    {
        return Ok(await _service.ListPendingAsync(User.GetUserId(), query, ct));
    }

    [HttpGet("history")]
    public async Task<ActionResult<PagedResult<PrescriptionDocumentVerificationDto>>> ListHistory(
        [FromQuery] PrescriptionDocumentHistoryQuery query,
        CancellationToken ct)
    {
        return Ok(await _service.ListHistoryAsync(User.GetUserId(), query, ct));
    }

    [HttpPut("{id:long}/verify")]
    public async Task<ActionResult<PrescriptionDocumentVerificationDto>> Verify(
        long id,
        [FromBody] PrescriptionDocumentDecisionRequest request,
        CancellationToken ct)
    {
        return Ok(await _service.VerifyAsync(User.GetUserId(), id, request, ct));
    }

    [HttpPut("{id:long}/reject")]
    public async Task<ActionResult<PrescriptionDocumentVerificationDto>> Reject(
        long id,
        [FromBody] PrescriptionDocumentDecisionRequest request,
        CancellationToken ct)
    {
        return Ok(await _service.RejectAsync(User.GetUserId(), id, request, ct));
    }
}