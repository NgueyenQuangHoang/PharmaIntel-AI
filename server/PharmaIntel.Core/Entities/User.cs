// =============================================================================
// Entity: User (Nguoi dung)
// Chuc nang: Luu thong tin tai khoan khach hang (ho ten, email, mat khau, OAuth).
// Quan he: 1:1 voi UserSetting | 1:N voi Address, PaymentMethod, Order, Prescription,
//          DiagnosticSession, CartItem, Notification, MedicationReminder, HealthMetric,
//          PharmacistChatSession, AiInsight, AuditLog, UserConsent.
// Luu y: Khong xoa cung - dung is_active de soft deactivate.
// =============================================================================
namespace PharmaIntel.Core.Entities;

public class User
{
    public long Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string? AvatarUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string AuthProvider { get; set; } = "local"; // local, google, apple
    public string? AuthProviderId { get; set; }
    public string Role { get; set; } = "user"; // user, admin, pharmacist
    public bool IsTermsAccepted { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public UserSetting? Setting { get; set; }
    public ICollection<UserConsent> Consents { get; set; } = new List<UserConsent>();
    public ICollection<Address> Addresses { get; set; } = new List<Address>();
    public ICollection<PaymentMethod> PaymentMethods { get; set; } = new List<PaymentMethod>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<DiagnosticSession> DiagnosticSessions { get; set; } = new List<DiagnosticSession>();
    public ICollection<DiagnosticResult> DiagnosticResults { get; set; } = new List<DiagnosticResult>();
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    public ICollection<MedicationReminder> MedicationReminders { get; set; } = new List<MedicationReminder>();
    public ICollection<HealthMetric> HealthMetrics { get; set; } = new List<HealthMetric>();
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<PharmacistChatSession> ChatSessions { get; set; } = new List<PharmacistChatSession>();
    public ICollection<AiInsight> AiInsights { get; set; } = new List<AiInsight>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    // 1:1 nullable - chi user co role 'pharmacist' moi co PharmacistProfile.
    public Pharmacist? PharmacistProfile { get; set; }
}
