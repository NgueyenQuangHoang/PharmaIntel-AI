// =============================================================================
// Program.cs - Entry point cua ung dung PharmaIntel AI API.
// Chuc nang: Cau hinh dependency injection, middleware pipeline, authentication,
//            CORS, Swagger, EF Core DbContext, va cac service cua ung dung.
// =============================================================================
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi;
using PharmaIntel.API.Extensions;
using PharmaIntel.API.Filters;
using PharmaIntel.API.Middleware;
using PharmaIntel.Infrastructure;
using PharmaIntel.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// === Validate cau hinh bat buoc (fail-fast) ===
// Chay truoc moi DI registration de loi xuat hien som va ro rang.
builder.Configuration.ValidateRequiredConfig(builder.Environment);

// === Infrastructure: DbContext + Auth + JWT services ===
builder.Services.AddInfrastructure(builder.Configuration);

// === Authentication - JWT Bearer ===
// Giu nguyen ten claim "sub", "email" thay vi map sang ClaimTypes
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
var jwtSettings = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key is not configured"))),
            // Vi DefaultMapInboundClaims = false, claim type giu nguyen "role" - can chi cho
            // [Authorize(Roles="admin")] biet doc claim nay.
            RoleClaimType = "role"
        };
    });
builder.Services.AddAuthorization();

// === CORS - Cho phep React client truy cap ===
// Cho phep cau hinh qua hai cach (gop lai, loai trung):
//   1) "Cors:AllowedOrigins": ["http://...", ...]   - dang mang trong appsettings.json
//   2) "Cors:AllowedOriginsCsv": "http://a, http://b" - dang CSV, tien khi truyen qua
//      env var (vd. docker-compose: Cors__AllowedOriginsCsv=...).
// Mang & env var indexed (Cors__AllowedOrigins__0) merge theo index nen kho clear
// gia tri tu appsettings.json - dung CSV de override sach se trong container.
var corsOrigins = ResolveCorsOrigins(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
    {
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

static string[] ResolveCorsOrigins(IConfiguration config)
{
    var fromArray = config.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
    var fromCsv = (config["Cors:AllowedOriginsCsv"] ?? string.Empty)
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    var merged = fromArray
        .Concat(fromCsv)
        .Select(o => o.Trim().TrimEnd('/'))
        .Where(o => !string.IsNullOrWhiteSpace(o))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();

    return merged.Length > 0 ? merged : ["http://localhost:5173"];
}

// === Rate Limiter (Phase 5: chong abuse AI/RAG endpoints) ===
// 3 policy:
//   - "ai-chat"        : 20 request/phut/user (chat AI + diagnostic message)
//   - "ai-ingest"      : 10 request/phut/admin (ingest/update/reindex knowledge)
//   - "ai-evaluation"  : 1 request/phut/admin (chay full eval suite - rat ton)
// PartitionKey la user identity, fallback IP.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("ai-chat", context => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: context.User.Identity?.Name
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? "anonymous",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 20,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        }));

    options.AddPolicy("ai-ingest", context => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: context.User.Identity?.Name
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? "anonymous",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        }));

    options.AddPolicy("ai-evaluation", context => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: context.User.Identity?.Name
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? "anonymous",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 1,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
});

// === Global Exception Handler + ProblemDetails (chuan RFC 7807) ===
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// === Health Checks ===
// - "live"  : tag liveness  - process con song khong (luon tra healthy)
// - "ready" : tag readiness - DB ket noi duoc khong
// Dung tag de filter endpoint:
//   GET /health        -> moi check (overall)
//   GET /health/live   -> chi tag "live"
//   GET /health/ready  -> chi tag "ready"
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("API process alive"), tags: ["live"])
    .AddDbContextCheck<PharmaIntelDbContext>(
        name: "database",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["ready"]);

// === Controllers + Validation Filter ===
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
})
// Tat auto-400 mac dinh cua ModelState - de ValidationFilter cua FluentValidation xu ly
.ConfigureApiBehaviorOptions(options => options.SuppressModelStateInvalidFilter = true);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PharmaIntel AI API",
        Version = "v1",
        Description = "API cho ung dung duoc pham thong minh PharmaIntel AI"
    });

    // Cau hinh Swagger ho tro JWT Bearer token
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhap JWT token. Vi du: eyJhbGciOiJI..."
    });
    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer"),
            new List<string>()
        }
    });
});

var app = builder.Build();

// === Bootstrap: auto-migrate + seed (dieu khien qua section "Bootstrap" trong appsettings) ===
await app.MigrateAndSeedAsync();

// === Middleware Pipeline ===
// Exception handler dat sat dau pipeline de bat moi exception phia sau
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "PharmaIntel AI API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowClient");
// Phuc vu file user upload qua wwwroot/uploads/... (vd: /uploads/prescriptions/.../xxx.jpg).
// Note: file public theo URL - chua co auth tren tang file. Voi don thuoc y te, prod
// nen chuyen sang endpoint co auth + signed URL.
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();

// === Health endpoints (KHONG yeu cau auth) ===
// Tra JSON gon: {"status":"Healthy","checks":[{"name":"database","status":"Healthy"}]}
var healthOptions = new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = HealthResponseWriter.WriteJson
};
app.MapHealthChecks("/health", healthOptions).AllowAnonymous();
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("live"),
    ResponseWriter = HealthResponseWriter.WriteJson
}).AllowAnonymous();
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("ready"),
    ResponseWriter = HealthResponseWriter.WriteJson
}).AllowAnonymous();

app.Run();
