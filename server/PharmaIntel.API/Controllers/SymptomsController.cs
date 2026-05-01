// =============================================================================
// Controller: SymptomsController
// Chuc nang: Doc danh muc trieu chung (public catalog - khong yeu cau auth).
// Endpoints:
//   GET /api/symptoms[?groupName=xxx]    list trieu chung (sap xep theo group + display order)
// =============================================================================
using Microsoft.AspNetCore.Mvc;
using PharmaIntel.Core.DTOs.Diagnostics;
using PharmaIntel.Core.Interfaces.Services;

namespace PharmaIntel.API.Controllers;

[ApiController]
[Route("api/symptoms")]
public class SymptomsController : ControllerBase
{
    private readonly ISymptomService _service;

    public SymptomsController(ISymptomService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<SymptomDto>>> List(
        [FromQuery] string? groupName, CancellationToken ct)
        => Ok(await _service.ListAsync(groupName, ct));
}
