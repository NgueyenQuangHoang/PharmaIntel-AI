// =============================================================================
// Service: AiFeedbackService
// Chuc nang: Create feedback (user) + list/review feedback (admin).
//            Rating + ReasonType chuan hoa lowercase + validate.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using PharmaIntel.Core.DTOs.AiFeedback;
using PharmaIntel.Core.Entities;
using PharmaIntel.Core.Exceptions;
using PharmaIntel.Core.Interfaces.Services;
using PharmaIntel.Infrastructure.Data;

namespace PharmaIntel.Infrastructure.Services;

public class AiFeedbackService : IAiFeedbackService
{
    private static readonly HashSet<string> AllowedReasons = new(StringComparer.OrdinalIgnoreCase)
    {
        "wrong_medication", "unsafe_advice", "not_helpful", "hallucination", "other"
    };

    private readonly PharmaIntelDbContext _db;

    public AiFeedbackService(PharmaIntelDbContext db)
    {
        _db = db;
    }

    public async Task<AiFeedbackDto> CreateAsync(
        long userId,
        CreateAiFeedbackRequest request,
        CancellationToken ct = default)
    {
        var rating = request.Rating.Trim().ToLowerInvariant();

        if (rating is not ("thumbs_up" or "thumbs_down"))
            throw new ValidationException("rating", "Rating phai la thumbs_up hoac thumbs_down");

        string? reasonType = null;
        if (!string.IsNullOrWhiteSpace(request.ReasonType))
        {
            reasonType = request.ReasonType.Trim().ToLowerInvariant();
            if (!AllowedReasons.Contains(reasonType))
                throw new ValidationException("reasonType", "ReasonType khong hop le");
        }

        var feedback = new AiResponseFeedback
        {
            UserId = userId,
            DiagnosticSessionId = request.DiagnosticSessionId,
            DiagnosticMessageId = request.DiagnosticMessageId,
            RagTraceId = request.RagTraceId,
            Rating = rating,
            ReasonType = reasonType,
            Comment = request.Comment?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _db.AiResponseFeedbacks.Add(feedback);
        await _db.SaveChangesAsync(ct);

        return ToDto(feedback);
    }

    public async Task<IReadOnlyList<AiFeedbackDto>> ListForAdminAsync(
        bool? isReviewed = null,
        string? rating = null,
        CancellationToken ct = default)
    {
        var query = _db.AiResponseFeedbacks.AsNoTracking();

        if (isReviewed.HasValue)
            query = query.Where(x => x.IsReviewed == isReviewed.Value);

        if (!string.IsNullOrWhiteSpace(rating))
            query = query.Where(x => x.Rating == rating);

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(200)
            .Select(x => new AiFeedbackDto
            {
                Id = x.Id,
                UserId = x.UserId,
                DiagnosticSessionId = x.DiagnosticSessionId,
                DiagnosticMessageId = x.DiagnosticMessageId,
                RagTraceId = x.RagTraceId,
                Rating = x.Rating,
                ReasonType = x.ReasonType,
                Comment = x.Comment,
                IsReviewed = x.IsReviewed,
                AdminNote = x.AdminNote,
                CreatedAt = x.CreatedAt,
                ReviewedAt = x.ReviewedAt
            })
            .ToListAsync(ct);
    }

    public async Task<AiFeedbackDto> MarkReviewedAsync(
        long feedbackId,
        ReviewAiFeedbackRequest request,
        CancellationToken ct = default)
    {
        var feedback = await _db.AiResponseFeedbacks
            .FirstOrDefaultAsync(x => x.Id == feedbackId, ct)
            ?? throw new NotFoundException("feedback", feedbackId);

        feedback.IsReviewed = true;
        feedback.AdminNote = request.AdminNote?.Trim();
        feedback.ReviewedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        return ToDto(feedback);
    }

    private static AiFeedbackDto ToDto(AiResponseFeedback x)
    {
        return new AiFeedbackDto
        {
            Id = x.Id,
            UserId = x.UserId,
            DiagnosticSessionId = x.DiagnosticSessionId,
            DiagnosticMessageId = x.DiagnosticMessageId,
            RagTraceId = x.RagTraceId,
            Rating = x.Rating,
            ReasonType = x.ReasonType,
            Comment = x.Comment,
            IsReviewed = x.IsReviewed,
            AdminNote = x.AdminNote,
            CreatedAt = x.CreatedAt,
            ReviewedAt = x.ReviewedAt
        };
    }
}
