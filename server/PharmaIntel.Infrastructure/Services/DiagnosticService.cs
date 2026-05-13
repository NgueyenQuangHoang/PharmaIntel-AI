// =============================================================================
// Service: DiagnosticService
// Chuc nang: Quan ly phien chan doan AI (user-scoped) + RAG Muc 1.
// Quan he:
//   DiagnosticSession 1:N DiagnosticMessage
//   DiagnosticSession N:N Symptom (qua DiagnosticSessionSymptom)
//   DiagnosticSession 1:1 DiagnosticResult
//   DiagnosticResult 1:N DiagnosticResultMedication -> Medication
// Quy tac:
//   - Create: yeu cau >=1 symptomId hop le. Tao session "in_progress" + attach
//     symptoms + system message tom tat + (optional) initial user message + AI
//     reply tu Gemini (RAG voi catalog thuoc).
//   - AddMessage: chi cho khi session "in_progress". Luu user message, retrieve
//     thuoc lien quan, goi Gemini sinh reply.
//   - Complete: in_progress -> analyzing -> retrieve thuoc -> Gemini analyze ->
//     loc SuggestedMedicationIds chi giu ID nam trong allowed (medicationContexts)
//     -> create result -> completed. Trong 1 transaction.
//   - Result chi tao 1 lan (UQ_diagnostic_results_session_id). Re-complete -> 409.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.DTOs.Diagnostics;
using PharmaIntel.Core.Entities;
using PharmaIntel.Core.Exceptions;
using PharmaIntel.Core.Interfaces.Services;
using PharmaIntel.Infrastructure.Data;

namespace PharmaIntel.Infrastructure.Services;

public class DiagnosticService : IDiagnosticService
{
    private const int MAX_SUGGESTED_MEDICATIONS = 3;

    private readonly PharmaIntelDbContext _db;
    private readonly IDiagnosticEngine _engine;
    private readonly IAiMedicationRetrievalService _medicationRetrieval;
    private readonly IKnowledgeRetrievalService _knowledgeRetrieval;
    private readonly IRagTraceService _ragTrace;
    private readonly ILogger<DiagnosticService> _logger;

    public DiagnosticService(
        PharmaIntelDbContext db,
        IDiagnosticEngine engine,
        IAiMedicationRetrievalService medicationRetrieval,
        IKnowledgeRetrievalService knowledgeRetrieval,
        IRagTraceService ragTrace,
        ILogger<DiagnosticService> logger)
    {
        _db = db;
        _engine = engine;
        _medicationRetrieval = medicationRetrieval;
        _knowledgeRetrieval = knowledgeRetrieval;
        _ragTrace = ragTrace;
        _logger = logger;
    }

    public async Task<DiagnosticSessionDto> CreateSessionAsync(long userId, CreateDiagnosticSessionRequest req, CancellationToken ct = default)
    {
        var distinctIds = req.SymptomIds.Distinct().ToList();
        var symptoms = await _db.Symptoms.AsNoTracking()
            .Where(s => distinctIds.Contains(s.Id))
            .ToListAsync(ct);

        if (symptoms.Count != distinctIds.Count)
        {
            var missing = distinctIds.Except(symptoms.Select(s => s.Id)).First();
            throw new NotFoundException("trieu chung", missing);
        }

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var session = new DiagnosticSession
        {
            UserId = userId,
            Status = "in_progress",
            CreatedAt = DateTime.UtcNow
        };
        _db.DiagnosticSessions.Add(session);
        await _db.SaveChangesAsync(ct);

        foreach (var s in symptoms)
        {
            _db.DiagnosticSessionSymptoms.Add(new DiagnosticSessionSymptom
            {
                SessionId = session.Id,
                SymptomId = s.Id
            });
        }

        var symptomsSummary = string.Join(", ", symptoms.Select(s => s.Name));

        // System summary chi them khi user co chon trieu chung. Neu chat tu do (khong
        // chon trieu chung) thi bo qua de chat trong sach.
        if (symptoms.Count > 0)
        {
            _db.DiagnosticMessages.Add(new DiagnosticMessage
            {
                SessionId = session.Id,
                SenderType = "system",
                Content = "Trieu chung da chon: " + symptomsSummary,
                SentAt = DateTime.UtcNow
            });
        }

        if (!string.IsNullOrWhiteSpace(req.InitialMessage))
        {
            var userText = req.InitialMessage.Trim();
            _db.DiagnosticMessages.Add(new DiagnosticMessage
            {
                SessionId = session.Id,
                SenderType = "user",
                Content = userText,
                SentAt = DateTime.UtcNow.AddMilliseconds(1)
            });

            // RAG: retrieve thuoc lien quan -> goi Gemini sinh reply
            var medicationContexts = await _medicationRetrieval.SearchRelevantMedicationsAsync(
                symptomsSummary,
                new[] { userText },
                ct);

            var aiReply = await _engine.GenerateChatReplyAsync(
                symptomsSummary,
                Array.Empty<string>(),
                userText,
                medicationContexts,
                ct);

            _db.DiagnosticMessages.Add(new DiagnosticMessage
            {
                SessionId = session.Id,
                SenderType = "ai",
                Content = aiReply,
                SentAt = DateTime.UtcNow.AddMilliseconds(2)
            });
        }

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return await BuildSessionDtoAsync(session.Id, ct);
    }

    public async Task<PagedResult<DiagnosticSessionListItemDto>> ListMySessionsAsync(long userId, DiagnosticSessionListQuery q, CancellationToken ct = default)
    {
        q.Normalize();

        var query = _db.DiagnosticSessions.AsNoTracking().Where(s => s.UserId == userId);

        if (!string.IsNullOrWhiteSpace(q.Status))
            query = query.Where(s => s.Status == q.Status);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(s => new DiagnosticSessionListItemDto
            {
                Id = s.Id,
                Status = s.Status,
                MessageCount = s.Messages.Count,
                Symptoms = s.SessionSymptoms.Select(ss => ss.Symptom.Name).ToList(),
                CreatedAt = s.CreatedAt,
                CompletedAt = s.CompletedAt
            })
            .ToListAsync(ct);

        return new PagedResult<DiagnosticSessionListItemDto>
        {
            Items = items,
            Page = q.Page,
            PageSize = q.PageSize,
            TotalCount = total
        };
    }

    public async Task<DiagnosticSessionDto> GetSessionByIdAsync(long userId, long sessionId, CancellationToken ct = default)
    {
        await EnsureSessionOwnedAsync(userId, sessionId, ct);
        return await BuildSessionDtoAsync(sessionId, ct);
    }

    public async Task<DiagnosticMessageDto> AddMessageAsync(long userId, long sessionId, AddDiagnosticMessageRequest req, CancellationToken ct = default)
    {
        var session = await _db.DiagnosticSessions
            .Include(s => s.SessionSymptoms).ThenInclude(ss => ss.Symptom)
            .Include(s => s.Messages)
            .FirstOrDefaultAsync(s => s.Id == sessionId, ct)
            ?? throw new NotFoundException("phien chan doan", sessionId);
        if (session.UserId != userId)
            throw new ForbiddenException("Phien chan doan khong thuoc ve ban");
        if (session.Status != "in_progress")
            throw new ConflictException($"Khong the gui tin nhan khi phien o trang thai '{session.Status}'");

        var userText = req.Content.Trim();

        var userMsg = new DiagnosticMessage
        {
            SessionId = sessionId,
            SenderType = "user",
            Content = userText,
            SentAt = DateTime.UtcNow
        };
        _db.DiagnosticMessages.Add(userMsg);

        var symptomsSummary = string.Join(", ", session.SessionSymptoms.Select(ss => ss.Symptom.Name));
        var conversationMessages = session.Messages
            .OrderBy(m => m.SentAt)
            .Select(m => $"{m.SenderType}: {m.Content}")
            .ToList();

        // Phase 5: do latency cua retrieve + generate de log vao RagTrace.
        var totalSw = System.Diagnostics.Stopwatch.StartNew();
        var retrievalSw = System.Diagnostics.Stopwatch.StartNew();
        string? errorType = null;

        // RAG Phase 1: retrieve thuoc lien quan (SQL keyword)
        var medicationContexts = await _medicationRetrieval.SearchRelevantMedicationsAsync(
            symptomsSummary,
            conversationMessages.Concat(new[] { $"user: {userText}" }).ToList(),
            ct);

        // RAG Phase 2: retrieve tai lieu y te lien quan (vector search Qdrant).
        // Failure cua vector layer khong duoc lam hong chat -> fallback rong.
        IReadOnlyList<KnowledgeContext> knowledgeContexts = Array.Empty<KnowledgeContext>();
        try
        {
            knowledgeContexts = await _knowledgeRetrieval.SearchAsync(userText, 5, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Knowledge retrieval that bai - tiep tuc chat khong co tai lieu context.");
            errorType = "knowledge_retrieval_failed";
        }

        retrievalSw.Stop();

        // Ghep tai lieu vao symptomsSummary tam thoi de gui sang Gemini cung context.
        var enrichedSymptomsSummary = symptomsSummary;
        if (knowledgeContexts.Count > 0)
        {
            var knowledgeBlock = string.Join("\n", knowledgeContexts.Select(x =>
                $"- [{x.SourceType}] {x.Title}: {x.Content}"));
            enrichedSymptomsSummary += "\n\nTAI LIEU Y TE LIEN QUAN:\n" + knowledgeBlock;
        }

        var generationSw = System.Diagnostics.Stopwatch.StartNew();
        var aiReply = await _engine.GenerateChatReplyAsync(
            enrichedSymptomsSummary,
            conversationMessages,
            userText,
            medicationContexts,
            ct);
        generationSw.Stop();
        totalSw.Stop();

        _db.DiagnosticMessages.Add(new DiagnosticMessage
        {
            SessionId = sessionId,
            SenderType = "ai",
            Content = aiReply,
            SentAt = DateTime.UtcNow.AddMilliseconds(1)
        });

        await _db.SaveChangesAsync(ct);

        // Phase 3: log trace de audit/eval. Failure khong duoc lam hong chat.
        try
        {
            await _ragTrace.LogAsync(
                sessionId, userText, medicationContexts, knowledgeContexts, aiReply,
                retrievalLatencyMs: (int)retrievalSw.ElapsedMilliseconds,
                generationLatencyMs: (int)generationSw.ElapsedMilliseconds,
                totalLatencyMs: (int)totalSw.ElapsedMilliseconds,
                errorType: errorType,
                ct: ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "RAG trace log that bai cho session {SessionId}", sessionId);
        }

        return new DiagnosticMessageDto
        {
            Id = userMsg.Id,
            SessionId = userMsg.SessionId,
            SenderType = userMsg.SenderType,
            Content = userMsg.Content,
            SentAt = userMsg.SentAt
        };
    }

    public async Task<DiagnosticSessionDto> CompleteSessionAsync(long userId, long sessionId, CancellationToken ct = default)
    {
        var session = await _db.DiagnosticSessions
            .Include(s => s.SessionSymptoms).ThenInclude(ss => ss.Symptom)
            .Include(s => s.Messages)
            .FirstOrDefaultAsync(s => s.Id == sessionId, ct)
            ?? throw new NotFoundException("phien chan doan", sessionId);

        if (session.UserId != userId)
            throw new ForbiddenException("Phien chan doan khong thuoc ve ban");
        if (session.Status != "in_progress")
            throw new ConflictException($"Khong the complete khi phien o trang thai '{session.Status}'");

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        // Step 1: chuyen sang analyzing
        session.Status = "analyzing";
        await _db.SaveChangesAsync(ct);

        var symptomsSummary = string.Join(", ", session.SessionSymptoms.Select(ss => ss.Symptom.Name));
        var conversationMessages = session.Messages
            .OrderBy(m => m.SentAt)
            .Select(m => $"{m.SenderType}: {m.Content}")
            .ToList();

        // Step 2: RAG retrieve thuoc lien quan
        var medicationContexts = await _medicationRetrieval.SearchRelevantMedicationsAsync(
            symptomsSummary,
            conversationMessages,
            ct);

        // Step 3: run engine voi context thuoc
        var engineReq = new DiagnosticEngineRequest
        {
            SymptomNames = session.SessionSymptoms.Select(ss => ss.Symptom.Name).ToList(),
            UserMessages = session.Messages.Where(m => m.SenderType == "user").Select(m => m.Content).ToList(),
            MedicationContexts = medicationContexts.ToList()
        };

        DiagnosticEngineResult engineResult;
        try
        {
            engineResult = await _engine.AnalyzeAsync(engineReq, ct);
        }
        catch
        {
            session.Status = "failed";
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            throw;
        }

        // Step 4: create result
        // UserId lay tu session.UserId (khong phai tu auth) de dam bao
        // result.user_id LUON khop voi session.user_id - tranh data drift.
        var result = new DiagnosticResult
        {
            SessionId = sessionId,
            UserId = session.UserId,
            AiConclusion = engineResult.AiConclusion,
            ConfidenceScore = engineResult.ConfidenceScore,
            RiskLevel = engineResult.RiskLevel,
            RedFlags = engineResult.RedFlags,
            RequiresDoctorVisit = engineResult.RequiresDoctorVisit,
            ModelName = engineResult.ModelName,
            ModelVersion = engineResult.ModelVersion,
            DiagnosedAt = DateTime.UtcNow
        };
        _db.DiagnosticResults.Add(result);
        await _db.SaveChangesAsync(ct);

        // Step 5: loc SuggestedMedicationIds chi giu ID nam trong allowed (RAG guard).
        // Du Gemini bia ID, backend van loai bo.
        var allowedMedicationIds = medicationContexts.Select(x => x.Id).ToHashSet();

        var selectedMedicationIds = engineResult.SuggestedMedicationIds
            .Where(id => allowedMedicationIds.Contains(id))
            .Distinct()
            .Take(MAX_SUGGESTED_MEDICATIONS)
            .ToList();

        // Fallback: neu Gemini khong goi y duoc gi -> lay top OTC tu medicationContexts
        if (selectedMedicationIds.Count == 0)
        {
            selectedMedicationIds = medicationContexts
                .Where(x => !x.IsPrescriptionRequired)
                .Select(x => x.Id)
                .Take(MAX_SUGGESTED_MEDICATIONS)
                .ToList();
        }

        var priority = 1;
        foreach (var medId in selectedMedicationIds)
        {
            _db.DiagnosticResultMedications.Add(new DiagnosticResultMedication
            {
                ResultId = result.Id,
                MedicationId = medId,
                Priority = priority++
            });
        }

        // Step 6: AI message tom tat ket luan
        _db.DiagnosticMessages.Add(new DiagnosticMessage
        {
            SessionId = sessionId,
            SenderType = "ai",
            Content = $"[Ket luan AI] {engineResult.AiConclusion} (do tin cay: {engineResult.ConfidenceScore}%)",
            SentAt = DateTime.UtcNow
        });

        // Step 7: chuyen sang completed
        session.Status = "completed";
        session.CompletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        await tx.CommitAsync(ct);

        return await BuildSessionDtoAsync(sessionId, ct);
    }

    public async Task<DiagnosticResultDto> GetResultByIdAsync(long userId, long resultId, CancellationToken ct = default)
    {
        var dto = await _db.DiagnosticResults.AsNoTracking()
            .Where(r => r.Id == resultId)
            .Select(r => new
            {
                r.Id, r.SessionId, r.UserId, r.AiConclusion, r.ConfidenceScore, r.RiskLevel,
                r.RedFlags, r.RequiresDoctorVisit, r.ModelName, r.ModelVersion, r.DiagnosedAt,
                Meds = r.ResultMedications.Select(rm => new DiagnosticSuggestedMedicationDto
                {
                    Id = rm.Id,
                    MedicationId = rm.MedicationId,
                    MedicationName = rm.Medication.Name,
                    Price = rm.Medication.Price,
                    DiscountPercent = rm.Medication.DiscountPercent,
                    ImageUrl = rm.Medication.ImageUrl,
                    IsPrescriptionRequired = rm.Medication.IsPrescriptionRequired,
                    Priority = rm.Priority
                }).OrderBy(x => x.Priority).ToList()
            })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException("ket qua chan doan", resultId);

        if (dto.UserId != userId)
            throw new ForbiddenException("Ket qua khong thuoc ve ban");

        return new DiagnosticResultDto
        {
            Id = dto.Id,
            SessionId = dto.SessionId,
            AiConclusion = dto.AiConclusion,
            ConfidenceScore = dto.ConfidenceScore,
            RiskLevel = dto.RiskLevel,
            RedFlags = dto.RedFlags,
            RequiresDoctorVisit = dto.RequiresDoctorVisit,
            ModelName = dto.ModelName,
            ModelVersion = dto.ModelVersion,
            DiagnosedAt = dto.DiagnosedAt,
            SuggestedMedications = dto.Meds
        };
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private async Task EnsureSessionOwnedAsync(long userId, long sessionId, CancellationToken ct)
    {
        var ownerId = await _db.DiagnosticSessions.AsNoTracking()
            .Where(s => s.Id == sessionId)
            .Select(s => (long?)s.UserId)
            .FirstOrDefaultAsync(ct);
        if (ownerId == null) throw new NotFoundException("phien chan doan", sessionId);
        if (ownerId != userId) throw new ForbiddenException("Phien chan doan khong thuoc ve ban");
    }

    private async Task<DiagnosticSessionDto> BuildSessionDtoAsync(long sessionId, CancellationToken ct)
    {
        var session = await _db.DiagnosticSessions.AsNoTracking()
            .Where(s => s.Id == sessionId)
            .Select(s => new
            {
                s.Id, s.Status, s.CreatedAt, s.CompletedAt,
                Symptoms = s.SessionSymptoms.Select(ss => ss.Symptom.Name).ToList(),
                Messages = s.Messages.OrderBy(m => m.SentAt).Select(m => new DiagnosticMessageDto
                {
                    Id = m.Id,
                    SessionId = m.SessionId,
                    SenderType = m.SenderType,
                    Content = m.Content,
                    SentAt = m.SentAt
                }).ToList(),
                Result = s.Result == null ? null : new
                {
                    s.Result.Id, s.Result.SessionId, s.Result.AiConclusion, s.Result.ConfidenceScore,
                    s.Result.RiskLevel, s.Result.RedFlags, s.Result.RequiresDoctorVisit,
                    s.Result.ModelName, s.Result.ModelVersion, s.Result.DiagnosedAt,
                    Meds = s.Result.ResultMedications.Select(rm => new DiagnosticSuggestedMedicationDto
                    {
                        Id = rm.Id,
                        MedicationId = rm.MedicationId,
                        MedicationName = rm.Medication.Name,
                        Price = rm.Medication.Price,
                        DiscountPercent = rm.Medication.DiscountPercent,
                        ImageUrl = rm.Medication.ImageUrl,
                        IsPrescriptionRequired = rm.Medication.IsPrescriptionRequired,
                        Priority = rm.Priority
                    }).OrderBy(x => x.Priority).ToList()
                }
            })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException("phien chan doan", sessionId);

        return new DiagnosticSessionDto
        {
            Id = session.Id,
            Status = session.Status,
            MessageCount = session.Messages.Count,
            Symptoms = session.Symptoms,
            CreatedAt = session.CreatedAt,
            CompletedAt = session.CompletedAt,
            Messages = session.Messages,
            Result = session.Result == null ? null : new DiagnosticResultDto
            {
                Id = session.Result.Id,
                SessionId = session.Result.SessionId,
                AiConclusion = session.Result.AiConclusion,
                ConfidenceScore = session.Result.ConfidenceScore,
                RiskLevel = session.Result.RiskLevel,
                RedFlags = session.Result.RedFlags,
                RequiresDoctorVisit = session.Result.RequiresDoctorVisit,
                ModelName = session.Result.ModelName,
                ModelVersion = session.Result.ModelVersion,
                DiagnosedAt = session.Result.DiagnosedAt,
                SuggestedMedications = session.Result.Meds
            }
        };
    }
}
