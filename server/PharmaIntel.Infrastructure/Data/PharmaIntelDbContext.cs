// =============================================================================
// DbContext chinh cua ung dung PharmaIntel AI.
// Chuc nang: Quan ly tat ca DbSet (30 bang), cau hinh quan he va rang buoc
//            thong qua EF Core Fluent API (cac file trong Data/Configurations).
// =============================================================================
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Infrastructure.Data;

public class PharmaIntelDbContext : DbContext
{
    public PharmaIntelDbContext(DbContextOptions<PharmaIntelDbContext> options)
        : base(options) { }

    // === Nguoi dung & bao mat ===
    public DbSet<User> Users => Set<User>();
    public DbSet<UserSetting> UserSettings => Set<UserSetting>();
    public DbSet<UserConsent> UserConsents => Set<UserConsent>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // === Nhan su y te ===
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Pharmacist> Pharmacists => Set<Pharmacist>();

    // === Thong bao ===
    public DbSet<Notification> Notifications => Set<Notification>();

    // === Dia chi & thanh toan ===
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();

    // === Danh muc thuoc ===
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Medication> Medications => Set<Medication>();

    // === Chan doan AI ===
    public DbSet<Symptom> Symptoms => Set<Symptom>();
    public DbSet<DiagnosticSession> DiagnosticSessions => Set<DiagnosticSession>();
    public DbSet<DiagnosticSessionSymptom> DiagnosticSessionSymptoms => Set<DiagnosticSessionSymptom>();
    public DbSet<DiagnosticMessage> DiagnosticMessages => Set<DiagnosticMessage>();
    public DbSet<DiagnosticResult> DiagnosticResults => Set<DiagnosticResult>();
    public DbSet<DiagnosticResultMedication> DiagnosticResultMedications => Set<DiagnosticResultMedication>();

    // === Don thuoc ===
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();
    public DbSet<PrescriptionDocument> PrescriptionDocuments => Set<PrescriptionDocument>();

    // === Nhac thuoc & suc khoe ===
    public DbSet<MedicationReminder> MedicationReminders => Set<MedicationReminder>();
    public DbSet<MedicationReminderLog> MedicationReminderLogs => Set<MedicationReminderLog>();
    public DbSet<HealthMetric> HealthMetrics => Set<HealthMetric>();

    // === Gio hang & don hang ===
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    // === Chat duoc si ===
    public DbSet<PharmacistChatSession> PharmacistChatSessions => Set<PharmacistChatSession>();
    public DbSet<PharmacistChatMessage> PharmacistChatMessages => Set<PharmacistChatMessage>();

    // === AI ===
    public DbSet<AiInsight> AiInsights => Set<AiInsight>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Ap dung tat ca cau hinh tu cac file IEntityTypeConfiguration trong assembly nay
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PharmaIntelDbContext).Assembly);

        // Chuyen tat ca ten cot sang snake_case de khop voi ERD (vi du AuthProvider -> auth_provider)
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties())
            {
                var current = property.GetColumnName();
                property.SetColumnName(ToSnakeCase(current));
            }
        }
    }

    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return Regex.Replace(input, "([a-z0-9])([A-Z])", "$1_$2").ToLowerInvariant();
    }
}
