// =============================================================================
// Entity: Pharmacist (Duoc si)
// Chuc nang: Danh muc duoc si ho tro xac minh don thuoc va chat voi nguoi dung.
// Quan he: 1:N voi PrescriptionDocument (xac minh) | 1:N voi PharmacistChatSession.
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class Pharmacist
{
    public long Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string? Specialization { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsOnline { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PrescriptionDocument> VerifiedDocuments { get; set; } = new List<PrescriptionDocument>();
    public ICollection<PharmacistChatSession> ChatSessions { get; set; } = new List<PharmacistChatSession>();
}
