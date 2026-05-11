using PharmaIntel.Core.DTOs.Common;

namespace PharmaIntel.Core.DTOs.Pharmacists;

public class PendingPrescriptionDocumentQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public void Normalize()
    {
        if (Page <= 0) Page = 1;
        if (PageSize <= 0) PageSize = 20;
        if (PageSize > 100) PageSize = 100;
    }
}

public class PrescriptionDocumentVerificationDto
{
    public long Id { get; set; }
    public long PrescriptionId { get; set; }
    public long UserId { get; set; }
    public string? UserFullName { get; set; }
    public string? PrescriptionTitle { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string VerificationStatus { get; set; } = string.Empty;
    public long? VerifiedByPharmacistId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class PrescriptionDocumentDecisionRequest
{
    public string? Notes { get; set; }
}