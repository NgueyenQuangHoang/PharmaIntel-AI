# PharmaIntel AI - Huong dan cau hinh moi truong

Tai lieu nay mo ta cach quan ly secret va cau hinh cho backend `PharmaIntel.API`.

## Nguyen tac

- **KHONG** commit secret (API key, JWT key, password, connection string thuc te) vao git.
- File `appsettings*.json` chi chua **default non-secret** + cau truc rong cho secret.
- **Dev local**: dung [User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) — luu o `%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json`, KHONG nam trong repo.
- **Production**: dung **environment variables** — set tren server hoac qua secret manager (Azure Key Vault, AWS Secrets Manager, ...).

## Thu tu uu tien (cao den thap, .NET mac dinh)

1. Environment variables (`Jwt__Key`, `ConnectionStrings__DefaultConnection`, ...)
2. User Secrets (chi load trong `Development`)
3. `appsettings.{Environment}.json`
4. `appsettings.json`

Gia tri uu tien cao se ghi de gia tri uu tien thap.

## Cac key bat buoc

| Key | Mo ta | Bat buoc |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string | Co (dev co default LocalDB trong `appsettings.Development.json`) |
| `Jwt:Key` | Khoa ky JWT, toi thieu 32 ky tu ngau nhien | Co |
| `Jwt:Issuer` / `Jwt:Audience` / `Jwt:ExpireMinutes` / `Jwt:RefreshExpireDays` | Thong tin token (refresh default 30 ngay) | Da co default |
| `Gemini:ApiKey` | Google Gemini API key | Co (khi `Gemini:Enabled=true`) |
| `Gemini:Model` / `Gemini:BaseUrl` / `Gemini:TimeoutSeconds` | Cau hinh Gemini | Da co default |
| `Cors:AllowedOrigins` | Mang origin frontend duoc phep | Da co default localhost |
| `Bootstrap:MigrateOnStartup` / `Bootstrap:Seed:Enabled` | Co tu chay migration & seed luc khoi dong | Da co default |

App **fail-fast** luc startup neu thieu mot trong cac key bat buoc, kem thong bao huong dan.

## Setup dev (lan dau)

```bash
cd server/PharmaIntel.API

# (Tuy chon) khoi tao user secrets - da khoi tao san trong .csproj
# dotnet user-secrets init

# JWT key (sinh chuoi ngau nhien 32+ ky tu)
dotnet user-secrets set "Jwt:Key" "thay-bang-chuoi-ngau-nhien-toi-thieu-32-ky-tu"

# Gemini API key (lay tu https://aistudio.google.com/app/apikey)
dotnet user-secrets set "Gemini:ApiKey" "AIza..."

# (Tuy chon) ghi de connection string neu khong dung LocalDB
# dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Database=...;..."
```

Cac lenh khac:

```bash
dotnet user-secrets list           # xem secret hien co
dotnet user-secrets remove "Jwt:Key"
dotnet user-secrets clear          # xoa toan bo
```

## Setup production

Set environment variables tren may chu (luu y `:` trong key duoc thay bang `__` o env var):

```bash
ConnectionStrings__DefaultConnection="Server=...;Database=PharmaIntelDB;User Id=...;Password=...;"
Jwt__Key="<chuoi-ngau-nhien-toi-thieu-32-ky-tu>"
Jwt__ExpireMinutes=60
Jwt__RefreshExpireDays=30
Gemini__ApiKey="AIza..."
Cors__AllowedOrigins__0="https://app.pharmaintel.example.com"
ASPNETCORE_ENVIRONMENT=Production
```

Tren Windows (PowerShell, persistent):

```powershell
[Environment]::SetEnvironmentVariable("Jwt__Key","...","Machine")
```

Tren Linux systemd:

```ini
[Service]
Environment="Jwt__Key=..."
Environment="ConnectionStrings__DefaultConnection=..."
```

## Sinh JWT key ngau nhien

PowerShell:
```powershell
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(48))
```

Bash:
```bash
openssl rand -base64 48
```

## .gitignore lien quan

Da chan o `server/.gitignore`:
- `appsettings.*.local.json`
- `secrets.json`
- `.env`, `.env.*` (giu lai `.env.example`)
- `*.pfx`, `*.key`

`secrets.json` cua User Secrets nam ngoai repo (`%APPDATA%\Microsoft\UserSecrets\...`) nen khong can them rule.

## Su co thuong gap

- **`InvalidOperationException: Cau hinh thieu hoac khong hop le`** luc start: lam theo huong dan trong message, hoac doc lai phan "Cac key bat buoc" o tren.
- **JWT key qua ngan**: yeu cau toi thieu 32 ky tu (HMAC-SHA256). Sinh lai bang lenh phia tren.
- **Gemini 401/403**: kiem tra `Gemini:ApiKey` co dung khong, hoac tat tam thoi bang `Gemini:Enabled=false`.
