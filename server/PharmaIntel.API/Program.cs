// =============================================================================
// Program.cs - Entry point cua ung dung PharmaIntel AI API.
// Chuc nang: Cau hinh dependency injection, middleware pipeline, authentication,
//            CORS, Swagger, EF Core DbContext, va cac service cua ung dung.
// =============================================================================
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using PharmaIntel.API.Extensions;
using PharmaIntel.API.Filters;
using PharmaIntel.API.Middleware;
using PharmaIntel.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:5173"])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// === Global Exception Handler + ProblemDetails (chuan RFC 7807) ===
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
