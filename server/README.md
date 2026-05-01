# PharmaIntel AI - Backend

Backend cho ung dung duoc pham + healthcare AI **PharmaIntel AI**: nguoi dung quan ly don thuoc,
nhac uong thuoc, theo doi chi so suc khoe, tu chan doan trieu chung qua AI, va dat thuoc online.
Kien truc Clean Architecture: API / Core / Infrastructure.

## Tech Stack

- **.NET 10** (LTS) + ASP.NET Core
- **Entity Framework Core 10** + SQL Server (LocalDB cho dev)
- **JWT Bearer Authentication** (60 phut, signing key trong appsettings)
- **FluentValidation** (auto-discover qua assembly scan)
- **Swashbuckle / Swagger** (chi bat o Development)
- **Global Exception Handler** + ProblemDetails (RFC 7807)

## Cau truc thu muc

```
server/
|-- PharmaIntel.API/              # Web API: Controllers, Program.cs, appsettings, Filters, Middleware
|-- PharmaIntel.Core/             # Domain: Entities (30), DTOs, Interfaces, Validators, Exceptions
|-- PharmaIntel.Infrastructure/   # EF Core: DbContext, Configurations, Migrations, Services, Seeders
|-- PharmaIntel.slnx              # Solution
|-- API_DOCUMENTATION.md          # Test guide cho moi endpoint (curl + JSON)
|-- agent.md                      # Backend architecture guidance
|-- erd_specification.md          # Spec 30 bang ERD
`-- README.md                     # File nay
```

## Yeu cau

- **.NET 10 SDK** (`dotnet --version` >= 10.0.0)
- **SQL Server** - LocalDB (di kem Visual Studio) hoac SQL Server full
- (Tuy chon) **EF Core CLI tools**: `dotnet tool install --global dotnet-ef`

## Quick start

```bash
# 1. Clone
git clone <repo-url>
cd server

# 2. Restore packages
dotnet restore

# 3. Run (auto-migrate + auto-seed lan dau)
dotnet run --project PharmaIntel.API
```

API chay tai **`http://localhost:5292`**, Swagger UI tai **`http://localhost:5292/swagger`**.

Lan chay dau, app se:
1. Tao database `PharmaIntelDB` (qua `Database.MigrateAsync`).
2. Seed du lieu mau: 12 trieu chung, 6 danh muc, 12 thuoc OTC, 3 bac si, 1 demo user.

Cac lan sau khong reset DB, seed khong duoc nhan ban (idempotent).

## Demo credentials

Sau khi seed, dang nhap voi 1 trong 2 tai khoan:

| Vai tro | Email | Password | Quyen |
| ------- | ----- | -------- | ----- |
| **User** | `demo@pharmaintel.ai` | `Demo@1234` | Mua hang, xem profile, quan ly don thuoc / nhac thuoc / chi so suc khoe |
| **Admin** | `admin@pharmaintel.ai` | `Admin@1234` | Tat ca quyen user + tao/sua/xoa Category, Medication, va cap nhat status don hang |

Demo user co san 1 dia chi default (Ha Noi) + 1 phuong thuc thanh toan COD - du de test
flow `Login -> AddToCart -> Checkout`.

### Vai tro (Role) trong JWT

JWT token chua claim `role` (`user` hoac `admin`). Endpoint admin dung
`[Authorize(Roles = "admin")]`. User thuong goi admin endpoint -> **403 Forbidden**.

Cac endpoint admin-only:
- `POST/PUT/DELETE /api/categories`
- `POST/PUT/DELETE /api/medications`
- `PUT /api/orders/{id}/status` (chuyen pending -> confirmed -> processing -> shipping -> delivered)

User co endpoint rieng `POST /api/orders/{id}/cancel` de huy don pending cua chinh minh.

## Database commands

```bash
# Reset toan bo DB (xoa va seed lai)
dotnet ef database drop --force --project PharmaIntel.Infrastructure --startup-project PharmaIntel.API
dotnet run --project PharmaIntel.API

# Apply migrations thu cong (khi tat MigrateOnStartup)
dotnet ef database update --project PharmaIntel.Infrastructure --startup-project PharmaIntel.API

# Tao migration moi (sau khi sua entity / EF config)
dotnet ef migrations add <Name> --project PharmaIntel.Infrastructure --startup-project PharmaIntel.API

# Xem SQL cua migration
dotnet ef migrations script --project PharmaIntel.Infrastructure --startup-project PharmaIntel.API
```

## Bootstrap config

`appsettings.json` co section `Bootstrap` dieu khien startup behavior:

```json
"Bootstrap": {
  "MigrateOnStartup": true,
  "Seed": { "Enabled": true }
}
```

- **Development** (mac dinh): `true/true` - chay xong la co data ngay.
- **Production** (`appsettings.Production.json`): `false/false` - admin tu chu dong migrate, khong bao gio seed.

## Environment variables (Production)

Khi deploy production, KHONG sua appsettings.Production.json (de trong) ma truyen qua env:

| Bien | Vi du |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__DefaultConnection` | `Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True` |
| `Jwt__Key` | Chuoi >= 32 ky tu, sinh ngau nhien |
| `Jwt__ExpireMinutes` | `60` (hoac thay theo policy) |
| `Cors__AllowedOrigins__0` | `https://app.your-domain.com` |
| `Cors__AllowedOrigins__1` | `https://admin.your-domain.com` |

> **JWT Key**: trong dev dung chuoi mac dinh trong `appsettings.json`. Khi deploy production,
> **luon override** qua env var de tranh leak qua git. Toi thieu 32 ky tu, khuyen 64+.

## API documentation

Toan bo endpoint kem mau curl/JSON luu o **`API_DOCUMENTATION.md`** (15 module, 60+ endpoint).

## Troubleshooting

| Loi | Cach xu ly |
| --- | --- |
| `error MSB3027: file is locked by PharmaIntel.API` | Kill process: `taskkill //F //IM PharmaIntel.API.exe` (Windows). |
| `JWT Key is not configured` | Kiem tra `Jwt:Key` trong appsettings hoac env `Jwt__Key` co set chua. |
| `IDX10720: Unable to create KeyedHashAlgorithm` | JWT Key < 32 byte (UTF-8). Dung chuoi dai hon. |
| `LocalDB error: instance does not exist` | `sqllocaldb create MSSQLLocalDB`, hoac doi connection string sang SQL Server full. |
| `Migrate failed: ... already exists` | DB co bang nhung khong co `__EFMigrationsHistory`. Drop DB roi run lai. |

## Lien ket

- **`API_DOCUMENTATION.md`** - test guide chi tiet
- **`agent.md`** - guideline kien truc, naming, quy uoc Vietnamese
- **`erd_specification.md`** - chi tiet 30 bang ERD
