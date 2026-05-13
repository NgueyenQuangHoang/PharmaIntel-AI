// =============================================================================
// Controller: AdminKnowledgeController
// Chuc nang: Ingest tai lieu y te vao knowledge base (RAG Phase 2). Admin only.
// Endpoints:
//   POST /api/admin/knowledge/ingest  -> chunk + embed + upsert Qdrant
// =============================================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaIntel.Core.DTOs.Knowledge;
using PharmaIntel.Core.Interfaces.Services;

namespace PharmaIntel.API.Controllers;

[ApiController]
[Authorize(Roles = "admin")]
[Route("api/admin/knowledge")]
public class AdminKnowledgeController : ControllerBase
{
    private readonly IKnowledgeIngestionService _ingestion;

    public AdminKnowledgeController(IKnowledgeIngestionService ingestion)
    {
        _ingestion = ingestion;
    }

    [HttpPost("ingest")]
    public async Task<ActionResult<object>> Ingest(
        [FromBody] IngestKnowledgeRequest request,
        CancellationToken ct)
    {
        if (request == null)
            return BadRequest(new { error = "Body khong duoc rong." });

        var documentId = await _ingestion.IngestTextAsync(
            request.Title,
            request.SourceType,
            request.Content,
            request.SourceUrl,
            ct);

        return Ok(new { documentId });
    }
}
