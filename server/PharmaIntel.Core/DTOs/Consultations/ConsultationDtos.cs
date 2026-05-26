// =============================================================================
// DTOs: Consultations
// Chuc nang: Cac DTO cho dat lich tu van - request tao moi, cap nhat trang thai,
// query danh sach va response cho ca user lan duoc si.
// =============================================================================
using PharmaIntel.Core.DTOs.Common;

namespace PharmaIntel.Core.DTOs.Consultations;

public class CreateConsultationRequest
{
    public long PharmacistId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public string? Note { get; set; }
}

public class UpdateConsultationStatusRequest
{
    public string Status { get; set; } = string.Empty; // accepted, rejected, completed, cancelled
    public string? ResponseNote { get; set; }
}

public class ConsultationDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string? UserEmail { get; set; }
    public string? UserPhoneNumber { get; set; }
    public long PharmacistId { get; set; }
    public string PharmacistName { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public string? Note { get; set; }
    public string Status { get; set; } = "pending";
    public string? ResponseNote { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ListConsultationsQuery : PagedQuery
{
    public string? Status { get; set; }
}
