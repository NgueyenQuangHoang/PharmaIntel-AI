# Frontend ↔ Backend Auth Integration

Tóm tắt việc kết nối React frontend với ASP.NET Core backend cho luồng xác thực, kèm hướng dẫn chạy dự án.

---

## 1. Mục tiêu đã đạt

- React đăng ký / đăng nhập thật vào backend.
- Backend trả JWT, frontend lưu token vào `localStorage`.
- Mọi request tiếp theo tự đính kèm header `Authorization: Bearer <token>`.
- `ProtectedRoute` chặn các route nội bộ khi chưa đăng nhập.
- Khi token hết hạn / sai (`401`): tự xoá token và redirect `/login`.
- Verify end-to-end bằng `GET /api/auth/me` (lấy user thật từ SQL Server).

---

## 2. Cấu trúc thay đổi

### Files mới

| File | Vai trò |
|---|---|
| [client/.env](client/.env) | `VITE_API_URL=http://localhost:5292/api` |
| [client/.env.example](client/.env.example) | Mẫu để commit |
| [client/src/features/auth/types.ts](client/src/features/auth/types.ts) | TS types khớp DTO backend (camelCase) |
| [client/src/features/auth/token-storage.ts](client/src/features/auth/token-storage.ts) | Wrapper `getToken/setToken/clearToken` quanh `localStorage` |
| [client/src/features/auth/auth-api.ts](client/src/features/auth/auth-api.ts) | Gọi `POST /auth/login`, `POST /auth/register`, `GET /auth/me` |
| [client/src/features/auth/auth-slice.ts](client/src/features/auth/auth-slice.ts) | Redux slice + thunks: `loginThunk`, `registerThunk`, `fetchMeThunk`, `logout` |
| [client/src/hooks/useAuth.ts](client/src/hooks/useAuth.ts) | Hook tiện dùng cho component |
| [client/src/components/ProtectedRoute.tsx](client/src/components/ProtectedRoute.tsx) | Guard route — redirect `/login` nếu chưa có token |

### Files sửa

| File | Thay đổi |
|---|---|
| [client/src/services/http-client.ts](client/src/services/http-client.ts) | Đổi fallback URL về `localhost:5292/api`; thêm request interceptor inject Bearer; response interceptor bắt 401 → clear token + dispatch `logout` + redirect `/login` (lazy import store để tránh circular) |
| [client/src/app/store.ts](client/src/app/store.ts) | Thêm `auth: authReducer` |
| [client/src/routes/index.tsx](client/src/routes/index.tsx) | Bọc nhánh `MainLayoutWrapper` bằng `ProtectedRoute` |
| [client/src/features/auth/components/LoginForm.tsx](client/src/features/auth/components/LoginForm.tsx) | Controlled inputs (`useState`), `onSubmit` dispatch `loginThunk`, loading state, error inline, toggle show password, navigate `/` khi thành công |
| [client/src/features/auth/components/RegisterForm.tsx](client/src/features/auth/components/RegisterForm.tsx) | Controlled, validate `password >= 8` + `confirmPassword` + `terms` ở client, gửi `isTermsAccepted: true`, navigate `/` khi thành công |
| [client/src/main.tsx](client/src/main.tsx) | Hydrate: nếu có token sẵn thì dispatch `fetchMeThunk()` lúc bootstrap |

### KHÔNG sửa

- `server/PharmaIntel.API/Program.cs` — CORS đã cấu hình sẵn cho `localhost:5173` qua policy `AllowClient`, thứ tự middleware đúng.
- Backend code & DB — task chỉ ở phía client.
- Các page `Home / Diagnostic / Medicine / Profile` — tách thành task sau khi wire data thật.

---

## 3. Quyết định đã chốt

| Hạng mục | Lựa chọn | Lý do |
|---|---|---|
| Protocol dev | HTTP `http://localhost:5292` | Tránh phải `dotnet dev-certs https --trust` |
| Token storage | `localStorage` | Giữ session qua reload, đơn giản. Backend chưa có refresh token |
| Phạm vi | Chỉ auth + `/auth/me` | Wire data domain (medications/cart...) ở task sau |
| 401 handling | Interceptor: clear + redirect `/login` | Chuẩn vì chưa có refresh endpoint |

---

## 4. Hướng dẫn khởi chạy

### Yêu cầu

- .NET 10 SDK
- Node.js 20+ và npm
- SQL Server (LocalDB / Express / instance đang chạy)
- Connection string đã cấu hình trong `server/PharmaIntel.API/appsettings.Development.json`

### Bước 1 — chuẩn bị database (chỉ lần đầu)

```bash
cd "server"
dotnet ef database update --project PharmaIntel.Infrastructure --startup-project PharmaIntel.API
```

### Bước 2 — chạy backend

Terminal 1:
```bash
cd "server"
dotnet run --project PharmaIntel.API --launch-profile http
```

Backend chạy tại:
- API: `http://localhost:5292`
- Swagger: `http://localhost:5292/swagger`
- Health: `http://localhost:5292/api/health`

### Bước 3 — chạy frontend

Terminal 2:
```bash
cd "client"
npm install     # chỉ lần đầu
npm run dev
```

Frontend chạy tại: `http://localhost:5173`

> File `client/.env` đã có sẵn `VITE_API_URL=http://localhost:5292/api`. Nếu backend chạy port khác, sửa file này rồi restart `npm run dev`.

---

## 5. Kịch bản test end-to-end

Mở `http://localhost:5173` trong trình duyệt và DevTools (F12):

1. **Guard hoạt động**: vào `/` → tự redirect `/login`.
2. **Đăng ký**: click "Register" → điền form → submit.
   - Network tab: `POST /api/auth/register` 200, response chứa `accessToken`.
   - App tự navigate `/`.
3. **Token được lưu**: DevTools → Application → Local Storage → key `pharmaintel.access_token` chứa JWT.
4. **Persist qua reload**: F5 trang `/` → vẫn ở `/` (hydrate qua `GET /api/auth/me`).
5. **Logout / clear token**: trong DevTools xóa key trên → reload → bị đẩy về `/login`.
6. **Đăng nhập lại**: dùng email/password vừa đăng ký → vào lại được `/`.
7. **Bắt 401**: sửa token thành chuỗi rác → reload → interceptor clear + redirect `/login`. Console không có lỗi CORS.
8. **(Optional) CORS**: kiểm Network → mọi request có header `Origin: http://localhost:5173` và backend trả `Access-Control-Allow-Origin`.

---

## 6. Lệnh hữu ích

```bash
# Frontend
cd client
npm run dev          # dev server (Vite) http://localhost:5173
npm run build        # production build (tsc -b && vite build)
npm run lint         # ESLint
npm run preview      # preview bản build

# Backend
cd server
dotnet run --project PharmaIntel.API --launch-profile http   # dev
dotnet build                                                  # build toàn solution
dotnet ef migrations add <Name> --project PharmaIntel.Infrastructure --startup-project PharmaIntel.API
dotnet ef database update --project PharmaIntel.Infrastructure --startup-project PharmaIntel.API

# Type check frontend (workaround do tsconfig dùng baseUrl đã deprecated trong TS6)
cd client
npx tsc -p tsconfig.app.json --noEmit --ignoreDeprecations 6.0
```

---

## 7. Ngoài phạm vi (giữ cho task sau)

- Refresh token / silent renewal — backend chưa có endpoint.
- Wire data thật cho **Medications / Cart / Prescriptions / Diagnostic / Profile**.
- OAuth Google / Apple (UI có nút nhưng backend hiện chỉ hỗ trợ `auth_provider='local'`).
- Cập nhật `server/API_DOCUMENTATION.md` — task này không thêm/sửa endpoint backend nên chưa bắt buộc.
- Sửa cảnh báo `baseUrl deprecated` trong `client/tsconfig.app.json` (thêm `"ignoreDeprecations": "6.0"`).
