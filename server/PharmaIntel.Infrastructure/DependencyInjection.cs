// =============================================================================
// Extension: DependencyInjection
// Chuc nang: Dang ky cac service cua tang Infrastructure (DbContext, Auth, JWT).
// =============================================================================
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PharmaIntel.Core.Entities;
using PharmaIntel.Core.Interfaces.Services;
using PharmaIntel.Core.Validators.Auth;
using PharmaIntel.Infrastructure.Data;
using PharmaIntel.Infrastructure.Data.Seeders;
using PharmaIntel.Infrastructure.Services;

namespace PharmaIntel.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<PharmaIntelDbContext>(options =>
            options.UseSqlServer(
                config.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly("PharmaIntel.Infrastructure")));

        services.Configure<JwtSettings>(config.GetSection("Jwt"));
        services.Configure<GeminiSettings>(config.GetSection("Gemini"));
        services.Configure<BankQrSettings>(config.GetSection("BankQr"));
        services.Configure<QdrantSettings>(config.GetSection("Qdrant"));
        services.Configure<EmbeddingSettings>(config.GetSection("Embedding"));

        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IMedicationService, MedicationService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IVietQrService, VietQrService>();
        services.AddScoped<IAddressService, AddressService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPrescriptionService, PrescriptionService>();
        services.AddScoped<IPharmacistPrescriptionVerificationService, PharmacistPrescriptionVerificationService>();
        services.AddScoped<IPharmacistPrescriptionItemService, PharmacistPrescriptionItemService>();
        services.AddScoped<IMedicationReminderService, MedicationReminderService>();
        services.AddScoped<IHealthMetricService, HealthMetricService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ISymptomService, SymptomService>();
        services.AddScoped<IAdminUserService, AdminUserService>();
        services.AddScoped<IAdminStatsService, AdminStatsService>();
        // Gemini AI engine (real). HttpClient duoc tao qua HttpClientFactory.
        services.AddHttpClient<IDiagnosticEngine, GeminiDiagnosticEngine>((sp, client) =>
        {
            var s = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<GeminiSettings>>().Value;
            client.Timeout = TimeSpan.FromSeconds(s.TimeoutSeconds <= 0 ? 30 : s.TimeoutSeconds);
        });
        services.AddScoped<IAiMedicationRetrievalService, AiMedicationRetrievalService>();

        // RAG Phase 2: knowledge base vector RAG.
        services.AddHttpClient<IEmbeddingService, GeminiEmbeddingService>((sp, client) =>
        {
            var s = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<GeminiSettings>>().Value;
            client.Timeout = TimeSpan.FromSeconds(s.TimeoutSeconds <= 0 ? 30 : s.TimeoutSeconds);
        });
        services.AddHttpClient<IVectorSearchService, QdrantVectorSearchService>();
        services.AddScoped<IKnowledgeIngestionService, KnowledgeIngestionService>();
        services.AddScoped<IKnowledgeRetrievalService, KnowledgeRetrievalService>();

        // RAG Phase 3: trace + evaluation.
        services.AddScoped<IRagTraceService, RagTraceService>();
        services.AddScoped<IRagEvaluationService, RagEvaluationService>();

        // RAG Phase 4: feedback + dashboard.
        services.AddScoped<IAiFeedbackService, AiFeedbackService>();
        services.AddScoped<IRagDashboardService, RagDashboardService>();

        services.AddScoped<IDiagnosticService, DiagnosticService>();

        // Seeder - chay luc startup neu Bootstrap:Seed:Enabled = true
        services.AddScoped<DataSeeder>();

        // FluentValidation: tim tat ca IValidator<T> trong assembly chua RegisterRequestValidator
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

        return services;
    }
}
