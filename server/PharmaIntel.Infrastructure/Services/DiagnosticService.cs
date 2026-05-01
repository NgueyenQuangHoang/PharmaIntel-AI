// =============================================================================
// Service: DiagnosticService
// Chuc nang: Quan ly phien chan doan AI (user-scoped).
// Quan he:
//   DiagnosticSession 1:N DiagnosticMessage
//   DiagnosticSession N:N Symptom (qua DiagnosticSessionSymptom)
//   DiagnosticSession 1:1 DiagnosticResult
//   DiagnosticResult 1:N DiagnosticResultMedication -> Medication
// Quy tac:
//   - Create: yeu cau >=1 symptomId hop le. Tao session status="in_progress" +
//     attach symptoms + system message tom tat trieu chung + (optional) initial
//     user message.
//   - AddMessage: chi cho khi session "in_progress". Luu user message, tu sinh
//     ai reply (mock).
//   - Complete: in_progress -> analyzing -> run engine -> create result + suggested
//     medications -> completed. Trong 1 transaction.
//   - Result chi tao 1 lan (UQ_diagnostic_results_session_id). Re-complete -> 409.
//   - Khi engine thay that (OpenAI/...), chi can register lai IDiagnosticEngine.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using PharmaIntel.Core.DTOs.Common;
using PharmaIntel.Core.DTOs.Diagnostics;
using PharmaIntel.Core.Entities;
using PharmaIntel.Core.Exceptions;
using PharmaIntel.Core.Interfaces.Services;
using PharmaIntel.Infrastructure.Data;

namespace PharmaIntel.Infrastructure.Services;

public class DiagnosticService : IDiagnosticService
{
    private readonly PharmaIntelDbContext _db;
    private readonly IDiagnosticEngine _engine;

    public DiagnosticService(PharmaIntelDbContext db, IDiagnosticEngine engine)
    {
        _db = db;
        _engine = engine;
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

        // System message tom tat trieu chung
        var summary = "Trieu chung da chon: " + string.Join(", ", symptoms.Select(s => s.Name));
        _db.DiagnosticMessages.Add(new DiagnosticMessage
        {
            SessionId = session.Id,
            SenderType = "system",
            Content = summary,
            SentAt = DateTime.UtcNow
        });

        // Initial user message (neu co)
        if (!string.IsNullOrWhiteSpace(req.InitialMessage))
        {
            _db.DiagnosticMessages.Add(new DiagnosticMessage
            {
                SessionId = session.Id,
                SenderType = "user",
                Content = req.InitialMessage.Trim(),
                SentAt = DateTime.UtcNow.AddMilliseconds(1)
            });

            // AI reply tu dong
            var aiReply = _engine.GenerateAutoReply(req.InitialMessage.Trim(), 1);
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
        var session = await _db.DiagnosticSessions.FirstOrDefaultAsync(s => s.Id == sessionId, ct)
                      ?? throw new NotFoundException("phien chan doan", sessionId);
        if (session.UserId != userId)
            throw new ForbiddenException("Phien chan doan khong thuoc ve ban");
        if (session.Status != "in_progress")
            throw new ConflictException($"Khong the gui tin nhan khi phien o trang thai '{session.Status}'");

        var existingUserMsgCount = await _db.DiagnosticMessages
            .CountAsync(m => m.SessionId == sessionId && m.SenderType == "user", ct);

        var userMsg = new DiagnosticMessage
        {
            SessionId = sessionId,
            SenderType = "user",
            Content = req.Content.Trim(),
            SentAt = DateTime.UtcNow
        };
        _db.DiagnosticMessages.Add(userMsg);

        var aiReply = _engine.GenerateAutoReply(req.Content.Trim(), existingUserMsgCount + 1);
        _db.DiagnosticMessages.Add(new DiagnosticMessage
        {
            SessionId = sessionId,
            SenderType = "ai",
            Content = aiReply,
            SentAt = DateTime.UtcNow.AddMilliseconds(1)
        });

        await _db.SaveChangesAsync(ct);

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

        // Step 1: chuyen sang analyzing de tracker biet dang chay engine
        session.Status = "analyzing";
        await _db.SaveChangesAsync(ct);

        // Step 2: run engine
        var engineReq = new DiagnosticEngineRequest
        {
            SymptomNames = session.SessionSymptoms.Select(ss => ss.Symptom.Name).ToList(),
            UserMessages = session.Messages.Where(m => m.SenderType == "user").Select(m => m.Content).ToList()
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

        // Step 3: create result + suggested medications
        var result = new DiagnosticResult
        {
            SessionId = sessionId,
            UserId = userId,
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

        var priority = 1;
        foreach (var medId in engineResult.SuggestedMedicationIds.Distinct())
        {
            _db.DiagnosticResultMedications.Add(new DiagnosticResultMedication
            {
                ResultId = result.Id,
                MedicationId = medId,
                Priority = priority++
            });
        }

        // Step 4: AI message tom tat ket luan
        _db.DiagnosticMessages.Add(new DiagnosticMessage
        {
            SessionId = sessionId,
            SenderType = "ai",
            Content = $"[Ket luan AI] {engineResult.AiConclusion} (do tin cay: {engineResult.ConfidenceScore}%)",
            SentAt = DateTime.UtcNow
        });

        // Step 5: chuyen sang completed
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
