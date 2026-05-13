// =============================================================================
// Controller: AdminKnowledgeController
// Chuc nang: Quan ly knowledge base (Phase 2 ingest + Phase 4 CRUD/reindex).
// Endpoints (admin only):
//   POST   /api/admin/knowledge/ingest            Ingest tai lieu moi
//   GET    /api/admin/knowledge                   List tat ca tai lieu
//   GET    /api/admin/knowledge/{id}              Get 1 tai lieu
//   PUT    /api/admin/knowledge/{id}              Update + re-chunk + re-embed
//   POST   /api/admin/knowledge/{id}/reindex      Re-embed chunk hien co
//   DELETE /api/admin/knowledge/{id}              Xoa tai lieu + chunks + vectors
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

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<KnowledgeDocumentDto>>> List(CancellationToken ct)
    {
        return Ok(await _ingestion.ListAsync(ct));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<KnowledgeDocumentDto>> Get(long id, CancellationToken ct)
    {
        return Ok(await _ingestion.GetByIdAsync(id, ct));
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<KnowledgeDocumentDto>> Update(
        long id,
        [FromBody] UpdateKnowledgeDocumentRequest request,
        CancellationToken ct)
    {
        return Ok(await _ingestion.UpdateAndReindexAsync(id, request, ct));
    }

    [HttpPost("{id:long}/reindex")]
    public async Task<IActionResult> Reindex(long id, CancellationToken ct)
    {
        await _ingestion.ReindexAsync(id, ct);
        return NoContent();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await _ingestion.DeleteAsync(id, ct);
        return NoContent();
    }
}
