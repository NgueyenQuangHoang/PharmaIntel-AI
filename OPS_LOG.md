# PharmaIntel AI - Ops Log

Ghi lai cac viec da lam o phan ops (DevOps / cau hinh / trien khai).
File nay duoc CAP NHAT MOI khi co thay doi ops moi.

> Cap nhat lan cuoi: 2026-05-26
> Trang thai: Phase 1-9 hoan tat.

---

## Tong quan

| Phase | Muc tieu | Trang thai |
|---|---|---|
| 1 | Chuan hoa cau hinh moi truong (tach secret) | Done |
| 2 | Docker hoa backend (.NET 10 API) | Done |
| 3 | Docker hoa frontend (React + Vite + nginx) | Done |
| 4 | Docker Compose chay full system | Done |
| 5 | Sua CORS cho Docker (CSV override) | Done |
| 6 | Health check API (/health, /live, /ready) | Done |
| 7 | GitHub Actions CI (backend + frontend) | Done |
| 8 | Docker build & push len GHCR | Done |
| 9 | Jenkins pipeline auto-deploy (pull GHCR -> compose up) | Done |

---

## Phase 9 - Jenkins auto-deploy

**Muc tieu:** Push master -> Jenkins tu pull image moi tu GHCR va deploy.

### Files them moi

| File | Vai tro |
|---|---|
| `Jenkinsfile` | Pipeline declarative: checkout -> pull -> deploy -> healthcheck |
| `docker-compose.prod.yml` | Override compose: dung image GHCR thay vi build local |

### Tien de tren server

1. Jenkins cai dat tren chinh server deploy (Ubuntu)
2. User `jenkins` da `usermod -aG docker jenkins` (chay `docker` khong can sudo)
3. File `.env` prod san o `/opt/pharmaintel/.env`
4. Image GHCR `pharmaintel-api` + `pharmaintel-web` set **Public** (GitHub Packages settings)
   - Hoac giu Private va `docker login ghcr.io` 1 lan duoi user `jenkins`

### Setup Jenkins job

1. New Item -> **Pipeline**
2. Build Triggers -> tick **GitHub hook trigger for GITScm polling**
3. Pipeline -> Definition: **Pipeline script from SCM**
   - SCM: Git, URL: `https://github.com/NgueyenQuangHoang/PharmaIntel-AI.git`
   - Branch: `master`, Script Path: `Jenkinsfile`
4. Tren GitHub repo -> Settings -> Webhooks -> them `http://<jenkins-url>/github-webhook/`

### Workflow

`git push master` -> webhook -> Jenkins build -> docker compose pull + up -d -> curl /health/ready -> done.

Khong rollback tu dong - container cu giu nguyen neu pull/up fail. Khi healthcheck fail, log API duoc in ra trong console Jenkins.

---

## Phase 1 - Chuan hoa cau hinh moi truong

**Muc tieu:** Khong commit secret. Dev dung User Secrets, Production dung environment variables.

### Files thay doi / them moi

| File | Vai tro |
|---|---|
| `server/PharmaIntel.API/appsettings.json` | Base committed - moi secret de rong (`Jwt:Key=""`, `Gemini:ApiKey=""`, connection rong) |
| `server/PharmaIntel.API/appsettings.Development.json` | Dev tweaks - chi giu logging + LocalDB connection |
| `server/PharmaIntel.API/appsettings.Production.json` | Khung prod - moi secret lay tu env vars |
| `server/PharmaIntel.API/Extensions/ConfigValidationExtensions.cs` | Fail-fast luc startup neu thieu key bat buoc |
| `server/PharmaIntel.API/Program.cs` | Goi `ValidateRequiredConfig` ngay sau `CreateBuilder` |
| `server/CONFIGURATION.md` | Huong dan setup dev (User Secrets) + prod (env vars) |

### Quy tac

1. **`appsettings.json`** = base committed, KHONG chua secret.
2. **`appsettings.Development.json`** = override non-secret cho dev (logging, LocalDB).
3. **User Secrets** (chi load trong `Development`) = secret cua dev local.
4. **Environment variables** (`Jwt__Key`, `ConnectionStrings__DefaultConnection`, `Gemini__ApiKey`, ...) = secret cua prod.

### Setup dev lan dau

```bash
cd server/PharmaIntel.API

# Sinh JWT key ngau nhien
# PowerShell:
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(48))
# Bash:
openssl rand -base64 48

# Luu vao User Secrets
dotnet user-secrets set "Jwt:Key" "<chuoi-vua-sinh>"
dotnet user-secrets set "Gemini:ApiKey" "AIza..."
# (tuy chon) ghi de connection string neu khong dung LocalDB
# dotnet user-secrets set "ConnectionStrings:DefaultConnection" "..."
```

### Validation luc startup

App **fail-fast** voi thong bao tieng Viet kem hint setup neu thieu:
- `ConnectionStrings:DefaultConnection`
- `Jwt:Key` (toi thieu 32 ky tu)
- `Gemini:ApiKey` (khi `Gemini:Enabled=true`)

---

## Phase 2 - Docker hoa backend

**Muc tieu:** Build image `.NET 10` API, multi-stage, non-root, expose port 8080.

### Files them moi

| File | Vai tro |
|---|---|
| `server/Dockerfile` | Multi-stage build: SDK 10 -> aspnet 10 |
| `server/.dockerignore` | Chan `bin/obj`, `.git`, docs, secrets |

### Diem chinh

- Stage 1 (`build`): `mcr.microsoft.com/dotnet/sdk:10.0` -> copy csproj truoc cho cache, restore, publish Release.
- Stage 2 (`final`): `mcr.microsoft.com/dotnet/aspnet:10.0`, chay user `app` (non-root), Kestrel listen `8080`, co HEALTHCHECK.
- Build context: `./server` (chua `.slnx` + 3 project).

### Build & run thu cong

```bash
docker build -t pharmaintel-api ./server

docker run --rm -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="Server=...;Database=PharmaIntelDB;User Id=...;Password=...;TrustServerCertificate=True;" \
  -e Jwt__Key="<32+ ky tu>" \
  -e Gemini__ApiKey="AIza..." \
  pharmaintel-api
```

---

## Phase 3 - Docker hoa frontend

**Muc tieu:** Build image React + Vite, serve qua nginx alpine.

### Files them moi

| File | Vai tro |
|---|---|
| `client/Dockerfile` | Multi-stage: node 22 build -> nginx 1.27 alpine |
| `client/nginx.conf` | Listen 8080, gzip, cache hashed asset, SPA fallback |
| `client/.dockerignore` | Chan `node_modules`, `dist`, `.env`, `.git` |

### Diem chinh

- `VITE_API_URL` duoc inline vao bundle JS luc BUILD (Vite dac diem). Truyen qua `--build-arg`. Doi URL = build lai image.
- nginx serve cong `8080` (non-privileged), bao mat header (X-Content-Type-Options, X-Frame-Options, Referrer-Policy).
- Cache strategy: `/assets/*` (Vite hash) cache 1 nam immutable, `index.html` no-store.
- SPA fallback: `try_files $uri $uri/ /index.html` cho react-router deep link.

### Build thu cong

```bash
# Local default
docker build -t pharmaintel-web ./client

# Prod voi API URL public
docker build -t pharmaintel-web \
  --build-arg VITE_API_URL=https://api.pharmaintel.example.com/api \
  ./client
```

---

## Phase 4 - Docker Compose full stack

**Muc tieu:** Mot lenh boot toan bo db + api + web.

### Files them moi

| File | Vai tro |
|---|---|
| `docker-compose.yml` | 3 service: `db` (SQL Server 2022), `api` (.NET 10), `web` (nginx) |
| `.env.example` | Mau bien moi truong cho compose |
| `.gitignore` (root) | Chan `.env` |

### Kien truc

```
browser ─► localhost:WEB_PORT (web/nginx) ─► localhost:API_PORT/api (api) ─► db:1433 (mssql, mang noi bo)
```

- `api` -> `db`: noi bo qua ten service `db,1433`. Doi `db` healthy moi start.
- `web` -> `api`: browser cua user goi tu host, nen `VITE_API_URL=http://localhost:API_PORT/api` (KHONG phai `http://api:8080`).
- Volume `pharmaintel-mssql-data` giu data SQL qua restart.
- API tu chay migration luc boot (`Bootstrap__MigrateOnStartup=true`).

### Su dung

```bash
cp .env.example .env
# sua SA_PASSWORD, JWT_KEY, GEMINI_API_KEY

docker compose up -d --build
docker compose logs -f api      # theo doi migrate + seed
docker compose ps               # xem trang thai
docker compose down             # stop, giu data
docker compose down -v          # stop + xoa volume DB
```

### Bien moi truong (.env)

| Bien | Mac dinh | Mo ta |
|---|---|---|
| `SA_PASSWORD` | (bat buoc) | Password sa cua SQL Server, phai strong |
| `DB_PORT` | 1433 | Cong DB expose ra host |
| `JWT_KEY` | (bat buoc) | Khoa ky JWT, >= 32 ky tu |
| `GEMINI_API_KEY` | (bat buoc) | Google Gemini key |
| `ASPNETCORE_ENVIRONMENT` | Production | |
| `API_PORT` | 5292 | Cong API expose ra host |
| `SEED_ON_STARTUP` | false | Co chay DataSeeder luc start khong |
| `WEB_PORT` | 5173 | Cong frontend expose ra host |
| `VITE_API_URL` | http://localhost:5292/api | URL browser goi sang API |
| `WEB_ORIGINS` | http://localhost:${WEB_PORT} | CSV danh sach origin duoc phep goi API (CORS) |

---

## Phase 5 - Sua CORS cho Docker

**Van de:** Cau hinh CORS qua mang trong appsettings.json (`["http://localhost:5173", "http://localhost:3000"]`) khong override sach duoc qua env var. ASP.NET Config merge mang theo INDEX, nen `Cors__AllowedOrigins__0=http://x` chi de len index 0, index 1 cua appsettings.json van con. Khi container chay tren prod, dat domain moi nhung port 3000 cu van duoc accept -> tac dung phu khong mong muon.

**Giai phap:** Them dang CSV `Cors:AllowedOriginsCsv`, gop voi mang cu, loai trung. Compose dung CSV de override sach se.

### Files thay doi

| File | Thay doi |
|---|---|
| `server/PharmaIntel.API/Program.cs` | Tach `ResolveCorsOrigins()` - merge `Cors:AllowedOrigins` (array) + `Cors:AllowedOriginsCsv` (CSV), distinct, normalize bo trailing slash |
| `docker-compose.yml` | Doi `Cors__AllowedOrigins__0` -> `Cors__AllowedOriginsCsv: ${WEB_ORIGINS}` |
| `.env.example` | Them bien `WEB_ORIGINS=http://localhost:5173,http://localhost:3000` |

### Cach dung

**Local docker-compose:**
```env
# .env
WEB_ORIGINS=http://localhost:5173,http://localhost:3000
```

**Prod (mot domain):**
```env
WEB_ORIGINS=https://app.pharmaintel.example.com
```

**Prod (nhieu domain):**
```env
WEB_ORIGINS=https://app.pharmaintel.example.com,https://staging.pharmaintel.example.com
```

### Quy tac

- CORS phai khop **CHINH XAC** scheme + host + port. `http://localhost:5173` khac `http://localhost:5173/` (da auto-trim trailing slash) va khac `https://localhost:5173`.
- Dev co the dung CSV trong User Secrets neu can: `dotnet user-secrets set "Cors:AllowedOriginsCsv" "http://localhost:5173"`.
- Khong dung `*` voi `AllowCredentials()` - browser se reject.

---

## Phase 6 - Health check API

**Muc tieu:** Co endpoint dat chuan de Docker, compose, va sau nay k8s probe biet API song & san sang phuc vu.

### Files thay doi / them moi

| File | Thay doi |
|---|---|
| `server/PharmaIntel.API/PharmaIntel.API.csproj` | Them package `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore` |
| `server/PharmaIntel.API/Program.cs` | Dang ky `AddHealthChecks` (self + DbContext), map 3 endpoint |
| `server/PharmaIntel.API/Extensions/HealthResponseWriter.cs` | Format JSON co structure thay vi plain "Healthy" |
| `server/Dockerfile` | HEALTHCHECK doi sang `/health/live` |
| `docker-compose.yml` | Them `healthcheck` cho `api` (dung `/health/ready`); `web.depends_on.api.condition: service_healthy` |

### 3 endpoint

| Endpoint | Tag | Y nghia | Dung cho |
|---|---|---|---|
| `GET /health` | (tat ca) | Tong hop moi check | Debug / monitoring chi tiet |
| `GET /health/live` | `live` | Process con song | Container HEALTHCHECK, k8s livenessProbe |
| `GET /health/ready` | `ready` | DB ket noi duoc, san sang nhan request | Compose/k8s readinessProbe, load balancer |

Cau hinh phan biet **liveness** (restart container neu chet) vs **readiness** (tam ngung nhan traffic):
- DB tam thoi xuong -> `/health/ready` fail -> bo khoi load balancer, **khong** restart container.
- Process treo -> `/health/live` fail -> orchestrator restart container.

### Response format

```json
{
  "status": "Healthy",
  "totalDurationMs": 12.4,
  "checks": [
    { "name": "database", "status": "Healthy", "durationMs": 8.1, "description": null, "error": null }
  ]
}
```

HTTP status: `200` neu Healthy/Degraded, `503` neu Unhealthy.

### Test thu

```bash
curl http://localhost:5292/health
curl http://localhost:5292/health/live
curl http://localhost:5292/health/ready
```

### Tac dong cua compose

`web` gio cho `api` healthy (DB ket noi duoc, migration xong) moi start. Boot order:
```
db (healthy) -> api (healthy) -> web
```

Khac voi truoc: `web` chi cho `api` "started" (process khoi dong) chu khong cho ready - co the gay 502 trong vai giay dau.

---

## Phase 7 - GitHub Actions CI

**Muc tieu:** Tu dong build & verify backend va frontend moi khi push/PR vao master.

### Files them moi

| File | Vai tro |
|---|---|
| `.github/workflows/backend-ci.yml` | CI cho `server/**` |
| `.github/workflows/frontend-ci.yml` | CI cho `client/**` |

### Backend CI

**Trigger:** push/PR `master` khi co thay doi trong `server/**` hoac chinh workflow.

**Job `build`:**
1. Setup .NET 10
2. Cache NuGet (`~/.nuget/packages`, key theo hash `*.csproj`)
3. `dotnet restore` -> `dotnet build -c Release`
4. `dotnet test` (tu dong skip neu chua co test project; phat hien qua glob `**/*.Tests.csproj`)
5. Upload test results (TRX) lam artifact

**Job `docker`:** (sau `build`)
- Buildx + build Docker image `pharmaintel-api:ci-<sha>`, KHONG push
- Cache GHA layer (`type=gha`) de lan sau nhanh hon

### Frontend CI

**Trigger:** push/PR `master` khi co thay doi trong `client/**` hoac chinh workflow.

**Job `build`:**
1. Setup Node 22 + cache npm theo `package-lock.json`
2. `npm ci`
3. `npm run lint` (ESLint)
4. `npm run build` (tsc + Vite)
5. Upload `client/dist/` lam artifact (giu 3 ngay)

**Job `docker`:** (sau `build`)
- Build Docker image `pharmaintel-web:ci-<sha>`, KHONG push
- Cache GHA layer

### Diem chinh

- **path filter**: backend khong chay khi chi sua client va nguoc lai - tiet kiem CI minutes.
- **workflow_dispatch**: cho phep chay tay tu UI Actions.
- **Khong push image**: chua co registry; phase sau co the them step push len GHCR/Docker Hub.
- **CI bao test fail**: neu them test project, CI tu dong chay va fail neu co loi.
- **`continue-on-error` KHONG dung**: tat ca step bat buoc pass.

### Cac phase mo rong (de xuat)

- Push image len `ghcr.io/<owner>/pharmaintel-{api,web}` khi merge vao master + tag version.
- Trivy scan image truoc khi push.
- Auto deploy len staging qua SSH/k8s sau khi CI pass.

---

## Phase 8 - Docker build & push len GHCR

**Muc tieu:** Tu dong build + push image len GitHub Container Registry sau khi merge vao master hoac tag version.

### Files them moi

| File | Vai tro |
|---|---|
| `.github/workflows/docker-build.yml` | Build + push api & web image len `ghcr.io/<owner>/pharmaintel-{api,web}` |

### Trigger

| Su kien | Tag image sinh ra |
|---|---|
| Push `master` | `:master`, `:sha-<7>`, `:latest` |
| Push tag `v1.2.3` | `:1.2.3`, `:1.2`, `:1`, `:sha-<7>`, `:latest` |
| `workflow_dispatch` | Chay tay, co the truyen `vite_api_url` |

### Yeu cau cau hinh repository

1. **Settings > Actions > General > Workflow permissions**: chon "Read and write permissions" (cho phep workflow ghi vao GHCR qua `GITHUB_TOKEN`).
2. **(Tuy chon) Secret `VITE_API_URL_PROD`**: URL API public cho frontend build. Vi du `https://api.pharmaintel.example.com/api`. Khong dat -> warning + fallback `http://localhost:5292/api` (image se sai khi deploy).
3. **Package visibility**: image GHCR mac dinh private. Settings > Packages > pharmaintel-api/web > Change visibility neu muon public.

### Diem chinh

- **2 job song song** (`api`, `web`) - giam thoi gian build.
- **`docker/metadata-action`** tu sinh tag chuan theo branch/semver/sha/latest.
- **Cache GHA scope rieng** (`scope=api`, `scope=web`) - khong dam nhau.
- **Frontend `VITE_API_URL`** uu tien: input thu cong > secret > placeholder.
- **Khong tu deploy** - chi push image. Deploy o phase sau (SSH/k8s/Render/Fly).

### Pull image va chay local

```bash
echo $GITHUB_TOKEN | docker login ghcr.io -u <github-username> --password-stdin

docker pull ghcr.io/<owner>/pharmaintel-api:latest
docker pull ghcr.io/<owner>/pharmaintel-web:latest

# Hoac sua docker-compose.yml -> doi `build:` thanh `image: ghcr.io/<owner>/...`
```

### Release flow de xuat

```bash
git tag v1.0.0
git push origin v1.0.0
# -> CI tu build & push: pharmaintel-api:1.0.0, :1.0, :1, :latest
```

---

## Canh bao bao mat / production

- `.env` thuc te **khong commit**.
- `SA_PASSWORD` luc that su deploy phai khac mau.
- Production khuyen dung:
  - SQL Server quan ly (Azure SQL, AWS RDS) thay vi self-host trong compose.
  - Reverse proxy (Nginx/Traefik/Caddy) + TLS truoc `web` va `api`.
  - Khong expose `db` ra host.
  - JWT key & Gemini key qua secret manager (Azure Key Vault, AWS Secrets Manager).

---

## 2026-05-19 - Google Sign-In (OAuth)

**Muc tieu:** Cho phep user dang nhap / dang ky bang tai khoan Google.

### Backend
- Them NuGet `Google.Apis.Auth` (v1.69.0) trong `PharmaIntel.Infrastructure`.
- DTO moi: `PharmaIntel.Core/DTOs/Auth/GoogleLoginRequest.cs` (`{ IdToken }`).
- `IAuthService.LoginWithGoogleAsync(...)` + impl trong `AuthService`:
  - `GoogleJsonWebSignature.ValidateAsync(idToken, { Audience = ClientId })`.
  - Tim user theo `(AuthProvider="google", AuthProviderId=sub)` -> fallback email.
  - Tao moi neu chua co (AuthProvider="google", khong co PasswordHash).
  - Link tai khoan local cung email -> "google" (giu PasswordHash).
- Endpoint moi: `POST /api/auth/google` trong `AuthController`.
- Options moi: `GoogleAuthSettings { ClientId }` -> registered o `DependencyInjection`.

### Cau hinh
- `appsettings.json` them section `"Google": { "ClientId": "" }`.
- Dev: `dotnet user-secrets set "Google:ClientId" "<web-client-id>"` (da set local).
- Prod: env var `Google__ClientId`.
- **KHONG can client_secret** cho luong web ID Token.

### Frontend
- Cai `@react-oauth/google`.
- `client/src/app/providers.tsx` wrap `<GoogleOAuthProvider clientId={...}>` (chi bat khi co `VITE_GOOGLE_CLIENT_ID`).
- `auth-api.ts`: `loginWithGoogle(idToken) -> POST /auth/google`.
- `auth-slice.ts`: `loginWithGoogleThunk` + extraReducer.
- `useAuth.ts`: them `loginWithGoogle`.
- `LoginForm.tsx`: thay nut Google disabled bang `<GoogleLogin onSuccess>` -> credential -> `loginWithGoogle(...)`.
- `client/.env.example`: them `VITE_GOOGLE_CLIENT_ID=`.

### Google Cloud Console - cau hinh OAuth Web Client
- Project: `pharmaintel-496801`.
- Authorized JavaScript origins: `https://medicalpharmal.com` (prod).
  Dev local can them: `http://localhost:5173`, `http://localhost:3000`.
- Authorized redirect URIs (cho flow code) hien co: `https://medicalpharmal.com`, `https://medicalpharmal.com/login`.
  Luong hien tai dung ID Token (One Tap / GIS button) **khong can** redirect URI - nhung neu sau nay chuyen sang code flow can them.

### Bao mat / luu y
- Client secret tu Google Cloud Console **khong dung** trong luong hien tai. Neu da bi lo, revoke + tao lai.
- Backend bat buoc verify audience claim cua ID token = `Google:ClientId` -> chong token tu app khac.

---

## Cac phase tiep theo (de xuat, chua lam)

- [ ] Reverse proxy gop `/api` + `/` cung 1 origin (bo luon CORS).
- [ ] CD: auto deploy len server/k8s sau khi push image.
- [ ] Trivy/CodeQL scan image truoc khi push.
- [ ] Logging tap trung (Seq/Loki) + metrics.
- [ ] Runtime config injection cho frontend (1 image, nhieu moi truong).
