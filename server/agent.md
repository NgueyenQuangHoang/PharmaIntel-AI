Được. Từ bước này mình sẽ hướng dẫn bạn làm backend theo hướng **fresher nhưng cấu trúc chuẩn doanh nghiệp**, không làm kiểu demo một file/controller gọi thẳng DbContext lung tung.

Ta sẽ dùng:

```text
ASP.NET Core Web API + SQL Server + Entity Framework Core + Clean Architecture đơn giản hóa
```

Nên dùng **ASP.NET Core**, không dùng ASP.NET MVC cũ / .NET Framework cũ. Microsoft hiện cũng định hướng ASP.NET Core để xây HTTP APIs, web apps và cloud services. Với version, nếu máy bạn mới setup thì nên dùng **.NET 8 LTS** hoặc **.NET 10 LTS**. .NET bản chẵn là LTS; Microsoft đang liệt kê .NET 10 được support đến 14/11/2028 và .NET 8 đến 10/11/2026. ([Microsoft Learn][1])

---

# 1. Mục tiêu backend của mình

Backend này sẽ phục vụ app **PharmaIntel AI** theo ERD bạn đã có.

Các module chính:

```text
Auth
Users / Profile
Medicine / Categories
Cart
Orders
Prescriptions
Diagnostic AI
Notifications
Medication Reminders
Health Metrics
```

Nhưng mình sẽ không nhảy vào code hết ngay. Làm doanh nghiệp thì phải đi theo thứ tự:

```text
Setup project chuẩn
→ Kết nối SQL Server
→ Tạo Domain Entities
→ Tạo DbContext
→ Tạo API mẫu
→ Auth JWT
→ CRUD từng module
→ Validation
→ Error handling
→ Logging
→ Testing
```

---

# 2. Cấu trúc project chuẩn doanh nghiệp

Mình khuyên bạn dùng cấu trúc này:

```text
PharmaIntel.Backend/
│
├── src/
│   ├── PharmaIntel.Api/
│   │   ├── Controllers/
│   │   ├── Middlewares/
│   │   ├── Extensions/
│   │   ├── Filters/
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   ├── PharmaIntel.Application/
│   │   ├── DTOs/
│   │   ├── Interfaces/
│   │   ├── Services/
│   │   ├── Validators/
│   │   ├── Common/
│   │   └── DependencyInjection.cs
│   │
│   ├── PharmaIntel.Domain/
│   │   ├── Entities/
│   │   ├── Enums/
│   │   ├── Common/
│   │   └── Exceptions/
│   │
│   └── PharmaIntel.Infrastructure/
│       ├── Persistence/
│       │   ├── AppDbContext.cs
│       │   ├── Configurations/
│       │   └── Migrations/
│       ├── Repositories/
│       ├── Services/
│       └── DependencyInjection.cs
│
├── tests/
│   ├── PharmaIntel.UnitTests/
│   └── PharmaIntel.IntegrationTests/
│
├── PharmaIntel.Backend.sln
└── README.md
```

Ý nghĩa đơn giản:

```text
Api             = nhận request, trả response
Application     = xử lý nghiệp vụ
Domain          = entity, enum, rule cốt lõi
Infrastructure  = database, external service, repository
```

Quy tắc đi làm rất quan trọng:

```text
Controller không xử lý nghiệp vụ nặng.
Controller không gọi SQL trực tiếp.
Domain không phụ thuộc EF Core.
Application không phụ thuộc ASP.NET.
Infrastructure mới biết SQL Server / EF Core.
```

ASP.NET Core có dependency injection built-in, nên mình sẽ dùng DI để inject service/repository vào controller thay vì tự `new` object thủ công. Đây là cách làm phổ biến trong ASP.NET Core. ([Microsoft Learn][2])

---

# 3. Công cụ bạn cần cài

Bạn đã có:

```text
SSMS
SQL Server LocalDB / SQL Server
ERD
```

Cần thêm:

```text
.NET SDK 8 hoặc 10
Visual Studio 2022 / Rider / VS Code
Postman hoặc Insomnia
Git
```

Kiểm tra .NET SDK:

```bash
dotnet --version
```

Nếu chưa có, tải .NET SDK từ trang chính thức của Microsoft.

---

# 4. Tạo solution backend

Mở terminal tại thư mục bạn muốn đặt source code, ví dụ:

```bash
cd D:\Projects
mkdir PharmaIntel.Backend
cd PharmaIntel.Backend
```

Tạo solution:

```bash
dotnet new sln -n PharmaIntel.Backend
```

Tạo các project:

```bash
mkdir src
cd src

dotnet new webapi -n PharmaIntel.Api
dotnet new classlib -n PharmaIntel.Application
dotnet new classlib -n PharmaIntel.Domain
dotnet new classlib -n PharmaIntel.Infrastructure
```

Quay lại root:

```bash
cd ..
```

Add project vào solution:

```bash
dotnet sln add src/PharmaIntel.Api/PharmaIntel.Api.csproj
dotnet sln add src/PharmaIntel.Application/PharmaIntel.Application.csproj
dotnet sln add src/PharmaIntel.Domain/PharmaIntel.Domain.csproj
dotnet sln add src/PharmaIntel.Infrastructure/PharmaIntel.Infrastructure.csproj
```

---

# 5. Setup dependency giữa các project

Chạy:

```bash
dotnet add src/PharmaIntel.Application/PharmaIntel.Application.csproj reference src/PharmaIntel.Domain/PharmaIntel.Domain.csproj

dotnet add src/PharmaIntel.Infrastructure/PharmaIntel.Infrastructure.csproj reference src/PharmaIntel.Application/PharmaIntel.Application.csproj
dotnet add src/PharmaIntel.Infrastructure/PharmaIntel.Infrastructure.csproj reference src/PharmaIntel.Domain/PharmaIntel.Domain.csproj

dotnet add src/PharmaIntel.Api/PharmaIntel.Api.csproj reference src/PharmaIntel.Application/PharmaIntel.Application.csproj
dotnet add src/PharmaIntel.Api/PharmaIntel.Api.csproj reference src/PharmaIntel.Infrastructure/PharmaIntel.Infrastructure.csproj
```

Dependency đúng sẽ là:

```text
Api → Application → Domain
Api → Infrastructure → Application → Domain
```

Không làm:

```text
Domain → Infrastructure
Domain → Api
Application → Api
```

---

# 6. Cài package cần thiết

## Cho Infrastructure

```bash
dotnet add src/PharmaIntel.Infrastructure package Microsoft.EntityFrameworkCore
dotnet add src/PharmaIntel.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer
dotnet add src/PharmaIntel.Infrastructure package Microsoft.EntityFrameworkCore.Design
```

## Cho Api

```bash
dotnet add src/PharmaIntel.Api package Microsoft.EntityFrameworkCore.Design
dotnet add src/PharmaIntel.Api package Swashbuckle.AspNetCore
dotnet add src/PharmaIntel.Api package Microsoft.AspNetCore.Authentication.JwtBearer
```

## Cho Application

```bash
dotnet add src/PharmaIntel.Application package FluentValidation
dotnet add src/PharmaIntel.Application package FluentValidation.DependencyInjectionExtensions
```

Về API style, Microsoft hiện có cả Minimal API và controller-based API. Minimal API phù hợp app nhỏ/microservices nhẹ; controller API vẫn rất dễ học, rõ ràng cho fresher, dễ tổ chức module, validation, Swagger, versioning. Microsoft docs cũng nêu controllers là một hướng chính để xây Web API bên cạnh Minimal APIs. ([Microsoft Learn][3])

Với project này, mình khuyên dùng **Controllers**.

---

# 7. Cấu hình connection string

Trong file:

```text
src/PharmaIntel.Api/appsettings.json
```

thêm:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=PharmaIntelDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "SecretKey": "THIS_IS_DEVELOPMENT_SECRET_KEY_CHANGE_IN_PRODUCTION_123456",
    "Issuer": "PharmaIntel",
    "Audience": "PharmaIntelClient",
    "ExpiryMinutes": 60
  },
  "AllowedHosts": "*"
}
```

Sau này production thì không để secret trong `appsettings.json`. Nhưng fresher dev local thì tạm được.

---

# 8. Tạo Base Entity

Tạo file:

```text
src/PharmaIntel.Domain/Common/BaseEntity.cs
```

```csharp
namespace PharmaIntel.Domain.Common;

public abstract class BaseEntity
{
    public long Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
```

Với các bảng cần audit nhiều hơn, sau này thêm:

```text
CreatedBy
UpdatedBy
IsDeleted
DeletedAt
```

Nhưng giai đoạn fresher/mvp thì chưa cần nhồi quá nhiều.

---

# 9. Tạo entity đầu tiên: User

Tạo file:

```text
src/PharmaIntel.Domain/Entities/User.cs
```

```csharp
using PharmaIntel.Domain.Common;

namespace PharmaIntel.Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public string AuthProvider { get; set; } = "local";

    public string? AuthProviderId { get; set; }

    public bool IsTermsAccepted { get; set; }

    public UserSetting? UserSetting { get; set; }

    public ICollection<Address> Addresses { get; set; } = new List<Address>();

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
```

Tạm thời `Address`, `Order`, `UserSetting` chưa có thì sẽ báo lỗi. Ta sẽ tạo tiếp.

---

# 10. Tạo vài entity nền

## UserSetting

```text
src/PharmaIntel.Domain/Entities/UserSetting.cs
```

```csharp
using PharmaIntel.Domain.Common;

namespace PharmaIntel.Domain.Entities;

public class UserSetting : BaseEntity
{
    public long UserId { get; set; }

    public string DarkMode { get; set; } = "system";

    public string Language { get; set; } = "vi";

    public bool NotificationEnabled { get; set; } = true;

    public bool ReminderSoundEnabled { get; set; } = true;

    public User User { get; set; } = null!;
}
```

## Address

```text
src/PharmaIntel.Domain/Entities/Address.cs
```

```csharp
using PharmaIntel.Domain.Common;

namespace PharmaIntel.Domain.Entities;

public class Address : BaseEntity
{
    public long UserId { get; set; }

    public string RecipientName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Province { get; set; } = string.Empty;

    public string District { get; set; } = string.Empty;

    public string Ward { get; set; } = string.Empty;

    public string StreetAddress { get; set; } = string.Empty;

    public bool IsDefault { get; set; }

    public User User { get; set; } = null!;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
```

## Category

```text
src/PharmaIntel.Domain/Entities/Category.cs
```

```csharp
using PharmaIntel.Domain.Common;

namespace PharmaIntel.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? Icon { get; set; }

    public int DisplayOrder { get; set; }

    public ICollection<Medication> Medications { get; set; } = new List<Medication>();
}
```

## Medication

```text
src/PharmaIntel.Domain/Entities/Medication.cs
```

```csharp
using PharmaIntel.Domain.Common;

namespace PharmaIntel.Domain.Entities;

public class Medication : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Dosage { get; set; }

    public string? Packaging { get; set; }

    public decimal Price { get; set; }

    public decimal DiscountPercent { get; set; }

    public long? CategoryId { get; set; }

    public string? UsageInstructions { get; set; }

    public string? Benefits { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsFeatured { get; set; }

    public bool IsBestSeller { get; set; }

    public bool IsPrescriptionRequired { get; set; }

    public int StockQuantity { get; set; }

    public Category? Category { get; set; }
}
```

## Order

```text
src/PharmaIntel.Domain/Entities/Order.cs
```

```csharp
using PharmaIntel.Domain.Common;

namespace PharmaIntel.Domain.Entities;

public class Order : BaseEntity
{
    public long UserId { get; set; }

    public long? AddressId { get; set; }

    public decimal Subtotal { get; set; }

    public decimal ShippingFee { get; set; }

    public decimal Total { get; set; }

    public string Status { get; set; } = "pending";

    public string? Note { get; set; }

    public User User { get; set; } = null!;

    public Address? Address { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
```

## OrderItem

```text
src/PharmaIntel.Domain/Entities/OrderItem.cs
```

```csharp
using PharmaIntel.Domain.Common;

namespace PharmaIntel.Domain.Entities;

public class OrderItem : BaseEntity
{
    public long OrderId { get; set; }

    public long MedicationId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalPrice { get; set; }

    public Order Order { get; set; } = null!;

    public Medication Medication { get; set; } = null!;
}
```

---

# 11. Tạo AppDbContext

Tạo file:

```text
src/PharmaIntel.Infrastructure/Persistence/AppDbContext.cs
```

```csharp
using Microsoft.EntityFrameworkCore;
using PharmaIntel.Domain.Entities;

namespace PharmaIntel.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<UserSetting> UserSettings => Set<UserSetting>();

    public DbSet<Address> Addresses => Set<Address>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Medication> Medications => Set<Medication>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUsers(modelBuilder);
        ConfigureUserSettings(modelBuilder);
        ConfigureAddresses(modelBuilder);
        ConfigureCategories(modelBuilder);
        ConfigureMedications(modelBuilder);
        ConfigureOrders(modelBuilder);
        ConfigureOrderItems(modelBuilder);
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(255).IsRequired();
            entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            entity.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
            entity.Property(x => x.AvatarUrl).HasColumnName("avatar_url").HasMaxLength(500);
            entity.Property(x => x.AuthProvider).HasColumnName("auth_provider").HasMaxLength(20).IsRequired();
            entity.Property(x => x.AuthProviderId).HasColumnName("auth_provider_id").HasMaxLength(255);
            entity.Property(x => x.IsTermsAccepted).HasColumnName("is_terms_accepted");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(x => x.Email).IsUnique();
        });
    }

    private static void ConfigureUserSettings(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserSetting>(entity =>
        {
            entity.ToTable("user_settings");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.DarkMode).HasColumnName("dark_mode").HasMaxLength(20).IsRequired();
            entity.Property(x => x.Language).HasColumnName("language").HasMaxLength(10).IsRequired();
            entity.Property(x => x.NotificationEnabled).HasColumnName("notification_enabled");
            entity.Property(x => x.ReminderSoundEnabled).HasColumnName("reminder_sound_enabled");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(x => x.User)
                .WithOne(x => x.UserSetting)
                .HasForeignKey<UserSetting>(x => x.UserId);
        });
    }

    private static void ConfigureAddresses(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Address>(entity =>
        {
            entity.ToTable("addresses");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.RecipientName).HasColumnName("recipient_name").HasMaxLength(255).IsRequired();
            entity.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(20).IsRequired();
            entity.Property(x => x.Province).HasColumnName("province").HasMaxLength(100).IsRequired();
            entity.Property(x => x.District).HasColumnName("district").HasMaxLength(100).IsRequired();
            entity.Property(x => x.Ward).HasColumnName("ward").HasMaxLength(100).IsRequired();
            entity.Property(x => x.StreetAddress).HasColumnName("street_address").HasMaxLength(500).IsRequired();
            entity.Property(x => x.IsDefault).HasColumnName("is_default");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(x => x.User)
                .WithMany(x => x.Addresses)
                .HasForeignKey(x => x.UserId);
        });
    }

    private static void ConfigureCategories(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(x => x.Slug).HasColumnName("slug").HasMaxLength(100).IsRequired();
            entity.Property(x => x.Icon).HasColumnName("icon").HasMaxLength(100);
            entity.Property(x => x.DisplayOrder).HasColumnName("display_order");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(x => x.Slug).IsUnique();
        });
    }

    private static void ConfigureMedications(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Medication>(entity =>
        {
            entity.ToTable("medications");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
            entity.Property(x => x.Description).HasColumnName("description");
            entity.Property(x => x.Dosage).HasColumnName("dosage").HasMaxLength(100);
            entity.Property(x => x.Packaging).HasColumnName("packaging").HasMaxLength(100);

            entity.Property(x => x.Price)
                .HasColumnName("price")
                .HasColumnType("decimal(12,2)");

            entity.Property(x => x.DiscountPercent)
                .HasColumnName("discount_percent")
                .HasColumnType("decimal(5,2)");

            entity.Property(x => x.CategoryId).HasColumnName("category_id");
            entity.Property(x => x.UsageInstructions).HasColumnName("usage_instructions");
            entity.Property(x => x.Benefits).HasColumnName("benefits");
            entity.Property(x => x.ImageUrl).HasColumnName("image_url").HasMaxLength(500);
            entity.Property(x => x.IsFeatured).HasColumnName("is_featured");
            entity.Property(x => x.IsBestSeller).HasColumnName("is_best_seller");
            entity.Property(x => x.IsPrescriptionRequired).HasColumnName("is_prescription_required");
            entity.Property(x => x.StockQuantity).HasColumnName("stock_quantity");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(x => x.Category)
                .WithMany(x => x.Medications)
                .HasForeignKey(x => x.CategoryId);
        });
    }

    private static void ConfigureOrders(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.AddressId).HasColumnName("address_id");

            entity.Property(x => x.Subtotal)
                .HasColumnName("subtotal")
                .HasColumnType("decimal(12,2)");

            entity.Property(x => x.ShippingFee)
                .HasColumnName("shipping_fee")
                .HasColumnType("decimal(12,2)");

            entity.Property(x => x.Total)
                .HasColumnName("total")
                .HasColumnType("decimal(12,2)");

            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
            entity.Property(x => x.Note).HasColumnName("note");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(x => x.User)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.UserId);

            entity.HasOne(x => x.Address)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.AddressId);
        });
    }

    private static void ConfigureOrderItems(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("order_items");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.OrderId).HasColumnName("order_id");
            entity.Property(x => x.MedicationId).HasColumnName("medication_id");
            entity.Property(x => x.Quantity).HasColumnName("quantity");

            entity.Property(x => x.UnitPrice)
                .HasColumnName("unit_price")
                .HasColumnType("decimal(12,2)");

            entity.Property(x => x.TotalPrice)
                .HasColumnName("total_price")
                .HasColumnType("decimal(12,2)");

            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(x => x.Order)
                .WithMany(x => x.OrderItems)
                .HasForeignKey(x => x.OrderId);

            entity.HasOne(x => x.Medication)
                .WithMany()
                .HasForeignKey(x => x.MedicationId);
        });
    }
}
```

Tạm thời mình đang viết mapping trực tiếp trong `AppDbContext` để bạn dễ học. Sau này chuẩn hơn sẽ tách mỗi bảng ra một file:

```text
Configurations/UserConfiguration.cs
Configurations/MedicationConfiguration.cs
Configurations/OrderConfiguration.cs
```

---

# 12. Đăng ký Infrastructure vào DI

Tạo file:

```text
src/PharmaIntel.Infrastructure/DependencyInjection.cs
```

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PharmaIntel.Infrastructure.Persistence;

namespace PharmaIntel.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });

        return services;
    }
}
```

---

# 13. Sửa Program.cs

Mở:

```text
src/PharmaIntel.Api/Program.cs
```

Sửa thành:

```csharp
using Microsoft.EntityFrameworkCore;
using PharmaIntel.Infrastructure;
using PharmaIntel.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "Healthy",
    application = "PharmaIntel API",
    timestamp = DateTime.UtcNow
}));

app.MapGet("/api/db-check", async (AppDbContext dbContext) =>
{
    var canConnect = await dbContext.Database.CanConnectAsync();

    return Results.Ok(new
    {
        database = "PharmaIntelDB",
        canConnect
    });
});

app.Run();
```

---

# 14. Chạy thử backend

Tại root solution:

```bash
dotnet build
dotnet run --project src/PharmaIntel.Api
```

Mở Swagger:

```text
https://localhost:<port>/swagger
```

Test:

```text
GET /api/health
GET /api/db-check
```

Nếu `/api/db-check` trả:

```json
{
  "database": "PharmaIntelDB",
  "canConnect": true
}
```

là backend đã kết nối DB thành công.

---

# 15. Chuẩn code doanh nghiệp mình sẽ áp dụng từ đây

Từ module tiếp theo trở đi, mỗi API sẽ đi theo pattern:

```text
Controller
→ Request DTO
→ Validator
→ Application Service
→ Repository / DbContext
→ Response DTO
```

Ví dụ với Medicine:

```text
GET /api/medications
GET /api/medications/{id}
POST /api/medications
PUT /api/medications/{id}
DELETE /api/medications/{id}
```

Không trả thẳng Entity ra ngoài. Luôn dùng DTO:

```text
Medication entity      = dữ liệu DB
MedicationResponseDto  = dữ liệu trả cho client
CreateMedicationRequest = dữ liệu client gửi lên
```

---

# 16. Quy tắc code để đi làm

Nhớ các rule này:

```text
Không để password plain text.
Không trả passwordHash ra API.
Không query database trong controller nếu logic phức tạp.
Không để connection string production trong source code.
Không dùng string magic lung tung nếu có thể chuyển thành enum/constant.
Không trả exception raw ra frontend.
Không để controller quá dài.
Không lẫn Entity với DTO.
Không dùng decimal thiếu precision cho tiền.
Không update toàn bộ object nếu chỉ update vài field.
```

---

# 17. Lộ trình code backend cho project này

Mình đề xuất thứ tự triển khai:

```text
Phase 1: Project Setup
- Solution structure
- SQL Server connection
- Swagger
- Health check
- Global exception middleware

Phase 2: Auth
- Register
- Login
- Password hashing
- JWT access token
- Current user endpoint

Phase 3: Medicine
- Category CRUD
- Medication CRUD
- Search/filter/pagination

Phase 4: Cart + Order
- Add to cart
- Update quantity
- Checkout
- Order history

Phase 5: Profile
- User profile
- Address management
- User settings

Phase 6: Prescription
- Prescriptions
- Prescription items
- Upload documents nếu cần

Phase 7: Diagnostic
- Diagnostic sessions
- Symptoms
- Messages
- Results
- Suggested medications

Phase 8: Production polish
- Logging
- Validation
- Error response chuẩn
- Pagination response chuẩn
- Unit test
- Integration test
```

---

# 18. Bước tiếp theo nên làm ngay

Bây giờ bạn hãy làm đúng 3 việc này:

```text
1. Tạo solution theo cấu trúc trên
2. Add references + packages
3. Chạy /api/health và /api/db-check
```

Sau khi chạy được, mình sẽ hướng dẫn bạn code module đầu tiên theo chuẩn doanh nghiệp:

```text
Auth: Register + Login + JWT
```

Đây là module quan trọng nhất để bạn học được backend thực tế.

[1]: https://learn.microsoft.com/en-us/lifecycle/products/microsoft-net-and-net-core?utm_source=chatgpt.com "Microsoft .NET and .NET Core - Microsoft Lifecycle"
[2]: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-10.0&utm_source=chatgpt.com "Dependency injection in ASP.NET Core | Microsoft Learn"
[3]: https://learn.microsoft.com/en-us/aspnet/core/tutorials/min-web-api?view=aspnetcore-10.0&utm_source=chatgpt.com "Tutorial: Create a Minimal API with ASP.NET Core"
