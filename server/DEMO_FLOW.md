# PharmaIntel AI - Demo Flow End-to-End

> File nay huong dan **chay thu toan bo flow** cua he thong sau khi `dotnet run` thanh cong.
> Du lieu mau (12 thuoc, 6 danh muc, 12 trieu chung, 3 bac si, demo + admin user) duoc seed
> tu dong khi DB rong (xem `README.md` cho cach reset).

## 0. Setup

```bash
# Lan dau hoac sau khi reset DB
dotnet run --project PharmaIntel.API
```

API: `http://localhost:5292` | Swagger UI: `http://localhost:5292/swagger`

### Tai khoan seeded

| Vai tro | Email | Password |
| ------- | ----- | -------- |
| User    | `demo@pharmaintel.ai`  | `Demo@1234`  |
| Admin   | `admin@pharmaintel.ai` | `Admin@1234` |

### Bien moi truong (cho cac curl ben duoi)

```bash
BASE=http://localhost:5292

# Login user
USER_TOKEN=$(curl -s -X POST $BASE/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"demo@pharmaintel.ai","password":"Demo@1234"}' \
  | grep -oE '"accessToken":"[^"]+"' | sed 's/"accessToken":"//;s/"$//')

# Login admin
ADMIN_TOKEN=$(curl -s -X POST $BASE/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@pharmaintel.ai","password":"Admin@1234"}' \
  | grep -oE '"accessToken":"[^"]+"' | sed 's/"accessToken":"//;s/"$//')

echo "USER token:  ${USER_TOKEN:0:40}..."
echo "ADMIN token: ${ADMIN_TOKEN:0:40}..."
```

---

## 1. Health check

```bash
curl -s $BASE/api/health
# {"status":"running","database":"connected","timestamp":"..."}
```

---

## 2. Public catalog (khong can token)

### Categories

```bash
curl -s "$BASE/api/categories?page=1&pageSize=10"
# 6 danh muc: Cam cum, Tieu hoa, Dau va ha sot, Vitamin va khoang chat, Da lieu, Phu nu va tre em
```

### Medications

```bash
curl -s "$BASE/api/medications?page=1&pageSize=20"
# 12 thuoc OTC voi gia + stock
```

### Symptoms (cho diagnostic AI)

```bash
curl -s $BASE/api/symptoms
# 12 trieu chung sap xep theo group + display order
```

---

## 3. Profile cua user (`demo@pharmaintel.ai`)

```bash
# Profile
curl -s $BASE/api/users/me -H "Authorization: Bearer $USER_TOKEN"
# Role = "user"

# Settings (auto-create record default neu chua co)
curl -s $BASE/api/users/me/settings -H "Authorization: Bearer $USER_TOKEN"
# darkMode=system, languageCode=vi, ...

# Address mac dinh (seed san)
curl -s "$BASE/api/addresses/my?page=1&pageSize=10" \
  -H "Authorization: Bearer $USER_TOKEN"
# 1 address: So 1 Pham Van Bach, Dich Vong, Cau Giay, Ha Noi (isDefault=true)
```

---

## 4. Mua hang (User)

### 4.1 Xem 1 thuoc cu the

```bash
curl -s $BASE/api/medications/1 -H "Authorization: Bearer $USER_TOKEN"
```

### 4.2 Them vao gio

```bash
curl -X POST $BASE/api/cart/items \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"medicationId":1,"quantity":2}'

curl -X POST $BASE/api/cart/items \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"medicationId":11,"quantity":1}'

# Xem gio
curl -s $BASE/api/cart -H "Authorization: Bearer $USER_TOKEN"
```

### 4.3 Lay addressId + paymentMethodId

```bash
curl -s "$BASE/api/addresses/my" -H "Authorization: Bearer $USER_TOKEN"
# Ghi nho ID dau tien (vi du 1)

# PaymentMethod chua co API rieng - lay tu DB hoac dung ID seed san:
# Demo user co PaymentMethod cod (ID = 1)
```

### 4.4 Checkout

```bash
curl -X POST $BASE/api/orders/checkout \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"addressId":1,"paymentMethodId":1}'
# 201 - tra ve order voi orderCode, status="pending", paymentStatus="cod_pending"
# Stock cua medication 1 va 11 da bi tru
```

### 4.5 Xem don hang cua minh

```bash
curl -s "$BASE/api/orders/my?page=1&pageSize=10" \
  -H "Authorization: Bearer $USER_TOKEN"
```

### 4.6 User huy don pending cua minh

```bash
ORDER_ID=1
curl -X POST $BASE/api/orders/$ORDER_ID/cancel \
  -H "Authorization: Bearer $USER_TOKEN"
# 200 - status="cancelled", stock restore
```

---

## 5. Don thuoc + Nhac uong thuoc (User)

### 5.1 Tao don thuoc moi

```bash
curl -X POST $BASE/api/prescriptions \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"doctorId":1,"title":"Don kham cam cum","prescribedDate":"2026-04-30"}'
# 201 - status="draft", auto snapshot doctorNameSnapshot tu BS Nguyen Van A
```

### 5.2 Them muc thuoc vao don

```bash
RX_ID=1
curl -X POST $BASE/api/prescriptions/$RX_ID/items \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"medicationId":1,"dosage":"500mg","frequency":"3 lan/ngay","duration":"5 ngay"}'
```

### 5.3 Kich hoat don (draft -> active)

```bash
curl -X PUT $BASE/api/prescriptions/$RX_ID \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"doctorId":1,"title":"Don kham cam cum","prescribedDate":"2026-04-30","status":"active"}'
# 200 - chuyen draft -> active (yeu cau co >= 1 item)
```

### 5.4 Tao lich nhac thuoc tu don

```bash
PI_ID=1   # PrescriptionItem ID vua tao
curl -X POST $BASE/api/medication-reminders \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d "{\"prescriptionItemId\":$PI_ID,\"frequencyType\":\"daily\",\"reminderTime\":\"08:00:00\"}"
# 201 - auto snapshot medicationName tu PrescriptionItem
```

### 5.5 Danh dau da uong

```bash
REMINDER_ID=1
curl -X POST $BASE/api/medication-reminders/$REMINDER_ID/logs \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"scheduledAt":"2026-04-30T08:00:00Z","status":"taken"}'
# 201 - completedAt server tu set = UtcNow
```

### 5.6 Lich su log

```bash
curl -s "$BASE/api/medication-reminders/$REMINDER_ID/logs" \
  -H "Authorization: Bearer $USER_TOKEN"
```

---

## 6. Chi so suc khoe (User)

```bash
# Ghi huyet ap
curl -X POST $BASE/api/health-metrics \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"metricType":"blood_pressure","valueNumber":120,"valueNumber2":80,"notes":"Sang som"}'

# Ghi can nang
curl -X POST $BASE/api/health-metrics \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"metricType":"weight","valueNumber":65.5}'

# List + filter theo type cho bieu do
curl -s "$BASE/api/health-metrics?metricType=blood_pressure&page=1&pageSize=20" \
  -H "Authorization: Bearer $USER_TOKEN"
```

---

## 7. Diagnostic AI (User)

### 7.1 Bat dau phien chan doan

```bash
# Dung trieu chung seed: 1=Sot, 4=Ho, 5=So mui
curl -X POST $BASE/api/diagnostics/sessions \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"symptomIds":[1,4,5],"initialMessage":"Toi bi sot 2 ngay nay"}'
# 201 - session "in_progress" voi 3 messages (system + user + ai reply)
```

### 7.2 Gui them tin nhan

```bash
SES_ID=1
curl -X POST $BASE/api/diagnostics/sessions/$SES_ID/messages \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"content":"Co them dau hong nhe va nhuc moi"}'
# 201 - user message luu, AI auto-reply
```

### 7.3 Hoan thanh + chay engine

```bash
curl -X POST $BASE/api/diagnostics/sessions/$SES_ID/complete \
  -H "Authorization: Bearer $USER_TOKEN"
# 200 - session "completed", co result voi:
#   aiConclusion: "Co kha nang ban dang bi cam cum thong thuong..."
#   confidenceScore: 75
#   riskLevel: "low"
#   suggestedMedications: [Paracetamol, Ibuprofen, ...] (top 3 match)
#   modelName: "PharmaIntel-MockEngine", modelVersion: "0.1-rule-based"
```

### 7.4 Test red-flag (emergency)

```bash
# 12 = Dau nguc trong seed
curl -X POST $BASE/api/diagnostics/sessions \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"symptomIds":[12],"initialMessage":"Dau nguc trai"}'

# Lay sessionId tu response, vi du 2
curl -X POST $BASE/api/diagnostics/sessions/2/complete \
  -H "Authorization: Bearer $USER_TOKEN"
# riskLevel="emergency", requiresDoctorVisit=true, redFlags ghi ro 115
```

### 7.5 Xem ket qua doc lap

```bash
curl -s $BASE/api/diagnostics/results/1 -H "Authorization: Bearer $USER_TOKEN"
```

---

## 8. Notifications (User)

> Phase nay backend chua tu sinh notification — insert tay vao DB de demo:
> ```sql
> INSERT INTO notifications (user_id, notification_type, title, body, is_read)
> VALUES (2, 'order_update', 'Don ORD-... da xac nhan', 'Don hang dang chuan bi.', 0);
> ```
> (UserId 2 la demo user neu admin la 1.)

```bash
# Dem chua doc
curl -s $BASE/api/notifications/unread-count -H "Authorization: Bearer $USER_TOKEN"

# List
curl -s "$BASE/api/notifications?isRead=false" -H "Authorization: Bearer $USER_TOKEN"

# Mark 1 noti da doc
curl -X PUT $BASE/api/notifications/1/read -H "Authorization: Bearer $USER_TOKEN"

# Mark all
curl -X PUT $BASE/api/notifications/read-all -H "Authorization: Bearer $USER_TOKEN"
```

---

## 9. Admin flow (`admin@pharmaintel.ai`)

### 9.1 Tao danh muc moi

```bash
curl -X POST $BASE/api/categories \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name":"Cham soc da","displayOrder":7}'
# 201 - admin OK
```

### 9.2 User goi cung endpoint -> 403

```bash
curl -i -X POST $BASE/api/categories \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name":"Cham soc da","displayOrder":7}'
# HTTP 403 Forbidden - role "user" khong duoc phep
```

### 9.3 Tao thuoc moi (admin)

```bash
curl -X POST $BASE/api/medications \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "sku":"MED-NEW-001",
    "name":"Vitamin D3 1000IU",
    "categoryId":4,
    "manufacturer":"Pharmacity",
    "price":85000,
    "discountPercent":0,
    "stockQuantity":200,
    "isActive":true,
    "isPrescriptionRequired":false
  }'
```

### 9.4 Cap nhat status don hang (admin)

```bash
# Dau tien tao 1 don bang USER_TOKEN, ghi nho ORDER_ID. Sau do admin chuyen status:
curl -X PUT $BASE/api/orders/$ORDER_ID/status \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"status":"confirmed"}'

curl -X PUT $BASE/api/orders/$ORDER_ID/status \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"status":"processing"}'

curl -X PUT $BASE/api/orders/$ORDER_ID/status \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"status":"shipping"}'

curl -X PUT $BASE/api/orders/$ORDER_ID/status \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"status":"delivered"}'
# Auto: COD + delivered -> paymentStatus = "paid"
```

### 9.5 Test invalid transition

```bash
# Khong cho nhay tu pending -> delivered
curl -i -X PUT $BASE/api/orders/2/status \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"status":"delivered"}'
# 409 Conflict - "Khong the chuyen tu 'pending' sang 'delivered'"
```

### 9.6 Test user goi admin endpoint

```bash
curl -i -X PUT $BASE/api/orders/1/status \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"status":"confirmed"}'
# 403 Forbidden
```

---

## 10. Negative tests (security guard)

### 10.1 Khong gui token

```bash
curl -i $BASE/api/users/me
# 401 Unauthorized
```

### 10.2 Token user A goi resource cua user B

```bash
# Tao user khac qua /api/auth/register, lay token, goi GET /api/orders/{id-cua-demo}
# -> 403 Forbidden ("Don hang nay khong thuoc ve ban")
```

### 10.3 Vi pham CHECK constraint

```bash
# health_metrics.metric_type sai
curl -i -X POST $BASE/api/health-metrics \
  -H "Authorization: Bearer $USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"metricType":"glucose","valueNumber":5}'
# 400 - "MetricType phai la mot trong: blood_pressure, heart_rate, ..."
```

### 10.4 Validation FluentValidation

```bash
curl -i -X POST $BASE/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"fullName":"a","email":"bad","password":"weak","isTermsAccepted":false}'
# 400 voi field-level errors: fullName, email, password (chua HOA/thuong/so), isTermsAccepted
```

---

## 11. Reset & cleanup

```bash
# Stop API: Ctrl+C trong terminal dang chay, hoac:
taskkill //F //IM PharmaIntel.API.exe   # Windows

# Reset toan bo DB (xoa va seed lai)
dotnet ef database drop --force \
  --project PharmaIntel.Infrastructure \
  --startup-project PharmaIntel.API

dotnet run --project PharmaIntel.API
```

---

## Module coverage trong demo nay

- [x] **Auth** (Login user + admin) - muc 0
- [x] **Health/DB** - muc 1
- [x] **Categories** (public list + admin CRUD) - muc 2, 9.1
- [x] **Medications** (public list + admin CRUD) - muc 2, 9.3
- [x] **Symptoms** (public catalog) - muc 2
- [x] **User Profile + Settings** - muc 3
- [x] **Addresses** (seed default) - muc 3
- [x] **Cart** (add + view) - muc 4.2
- [x] **Orders** (checkout, list, cancel user, admin status) - muc 4, 9.4
- [x] **Prescriptions** (CRUD + items + status transition) - muc 5.1-5.3
- [x] **Medication Reminders** (CRUD + logs) - muc 5.4-5.6
- [x] **Health Metrics** (blood_pressure, weight) - muc 6
- [x] **Diagnostic AI** (mock engine, normal + emergency) - muc 7
- [x] **Notifications** (read + count) - muc 8
- [x] **Role / Admin Authorization** - muc 9 (xuyen suot)
- [x] **Validation + ProblemDetails** - muc 10

---

## Lien ket

- `README.md` - cau hinh moi truong, command DB
- `API_DOCUMENTATION.md` - dac ta day du tung endpoint kem mau request/response/loi
- `agent.md` - guideline kien truc va naming
- `erd_specification.md` - spec 30 bang ERD
