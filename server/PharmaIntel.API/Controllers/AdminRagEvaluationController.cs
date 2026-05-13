// =============================================================================
// Controller: AdminRagEvaluationController
// Chuc nang: Chay bo test eval RAG (Phase 3) - admin only.
// Endpoints:
//   POST /api/admin/rag-evaluation/run -> tra ve pass/fail per case + tong hop
// =============================================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaIntel.Core.Interfaces.Services;

namespace PharmaIntel.API.Controllers;

[ApiController]
[Authorize(Roles = "admin")]
[Route("api/admin/rag-evaluation")]
public class AdminRagEvaluationController : ControllerBase
{
    private readonly IRagEvaluationService _evaluation;

    public AdminRagEvaluationController(IRagEvaluationService evaluation)
    {
        _evaluation = evaluation;
    }

    [HttpPost("run")]
    public async Task<IActionResult> Run(CancellationToken ct)
    {
        var results = await _evaluation.RunAsync(ct);

        return Ok(new
        {
            total = results.Count,
            passed = results.Count(x => x.Passed),
            failed = results.Count(x => !x.Passed),
            passRate = results.Count == 0
                ? 0
                : Math.Round(results.Count(x => x.Passed) * 100.0 / results.Count, 2),
            results
        });
    }
}
