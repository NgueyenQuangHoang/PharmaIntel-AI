# PharmaIntel AI - API Testing Guide

> File nay liet ke toan bo cac endpoint cua API kem JSON mau de test va vi du ket qua tra ve.
> Dung de tu test lai sau khi co thay doi.

---

## 0. Chuan bi

### Khoi dong API

```bash
cd "D:/PharmaIntel AI/server/PharmaIntel.API"
dotnet run --launch-profile http
```

API se chay tai: **`http://localhost:5292`**
Swagger UI: **`http://localhost:5292/swagger`**

### Connection string

`appsettings.json` -> `(localdb)\MSSQLLocalDB` / `PharmaIntelDB`

### Reset database (khi can)

```bash
cd "D:/PharmaIntel AI/server"
dotnet ef database drop --force --project PharmaIntel.Infrastructure --startup-project PharmaIntel.API
dotnet ef database update --project PharmaIntel.Infrastructure --startup-project PharmaIntel.API
```

---

## 0.1 Chuan format loi (ProblemDetails - RFC 7807)

Toan bo loi tra ve theo dinh dang `application/problem+json` voi cac truong:

| Truong       | Y nghia                                                |
| ------------ | ------------------------------------------------------ |
| `title`      | Tieu de loi (vi du "Du lieu khong hop le")             |
| `status`     | HTTP status code                                       |
| `detail`     | Mo ta cu the                                           |
| `instance`   | Path da goi                                            |
| `errorType`  | Ma loi noi bo (`validation_error`, `conflict`, ...)    |
| `traceId`    | Trace id de tra log                                    |
| `errors`     | (chi co o 400 validation) dictionary `field -> [msg]`  |

### Bang status codes

| Code | Khi nao                            | `errorType`        |
| ---- | ---------------------------------- | ------------------ |
| 400  | Validation fail (FluentValidation) | `validation_error` |
| 401  | Sai email/password, token sai/het han | `unauthorized`  |
| 403  | Khong du quyen                     | `forbidden`        |
| 404  | Khong tim thay tai nguyen          | `not_found`        |
| 409  | Xung dot (vi du email ton tai)     | `conflict`         |
| 500  | Loi he thong khong mong muon       | `internal_error`   |

### Vi du response 400 (validation)

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Du lieu khong hop le",
  "status": 400,
  "detail": "Du lieu khong hop le",
  "instance": "/api/auth/register",
  "errors": {
    "fullName": ["Ho ten toi thieu 2 ky tu"],
    "email": ["Email khong hop le"],
    "password": [
      "Mat khau toi thieu 8 ky tu",
      "Mat khau phai co it nhat 1 chu HOA",
      "Mat khau phai co it nhat 1 chu so"
    ],
    "isTermsAccepted": ["Phai dong y dieu khoan su dung"]
  },
  "errorType": "validation_error",
  "traceId": "0HNL5TPVK0O22:00000001"
}
```

### Vi du response 409 (conflict)

```json
{
  "title": "Xung dot du lieu",
  "status": 409,
  "detail": "Email da duoc su dung",
  "instance": "/api/auth/register",
  "errorType": "conflict",
  "traceId": "0HNL5TPVK0O23:00000001"
}
```

### Vi du response 500

```json
{
  "title": "Loi he thong",
  "status": 500,
  "detail": "Da xay ra loi khong mong muon. Vui long thu lai.",
  "instance": "/api/...",
  "errorType": "internal_error",
  "traceId": "..."
}
```

> O moi truong **Development**, `detail` se chua full stack trace de debug.

---

## 1. Health & System

### 1.1 `GET /api/health`

Kiem tra API co dang chay va co ket noi duoc database hay khong.

**Request**

```bash
curl http://localhost:5292/api/health
```

**Response 200**

```json
{
  "status": "running",
  "database": "connected",
  "timestamp": "2026-04-29T11:02:01.3953211Z"
}
```

> `database` = `"disconnected"` neu mat ket noi DB. API van tra ve 200.

---

### 1.2 `GET /api/db-check`

Kiem tra chi tiet database: provider, ten DB, danh sach 30 bang, migration da apply.

**Request**

```bash
curl http://localhost:5292/api/db-check
```

**Response 200**

```json
{
  "status": "ok",
  "database": "connected",
  "provider": "Microsoft.EntityFrameworkCore.SqlServer",
  "databaseName": "PharmaIntelDB",
  "tableCount": 30,
  "tables": ["addresses", "ai_insights", "..."],
  "appliedMigrations": ["20260429104815_InitialCreate"],
  "pendingMigrations": [],
  "timestamp": "2026-04-29T11:02:02.0090909Z"
}
```

**Response 503** (DB khong ket noi)

```json
{
  "status": "error",
  "database": "disconnected",
  "message": "Khong the ket noi den database"
}
```

---

## 2. Auth (Xac thuc)

> Cac endpoint duoi day cho phep user dang ky / dang nhap. Tra ve **JWT Bearer token** dung de goi cac endpoint co `[Authorize]`.
>
> **Cach gui token**: them header `Authorization: Bearer <accessToken>`.
> **Het han**: 60 phut (cau hinh trong `appsettings.json` -> `Jwt:ExpireMinutes`).

---

### 2.1 `POST /api/auth/register`

Dang ky tai khoan moi (auth_provider = `local`). Email duoc luu lowercase.

**Request body**

```json
{
  "fullName": "Nguyen Van A",
  "email": "test@example.com",
  "password": "Password123",
  "isTermsAccepted": true
}
```

**Curl**

```bash
curl -X POST http://localhost:5292/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"fullName":"Nguyen Van A","email":"test@example.com","password":"Password123","isTermsAccepted":true}'
```

**Validation** (`RegisterRequestValidator`)

| Field             | Rang buoc                                                                      |
| ----------------- | ------------------------------------------------------------------------------ |
| `fullName`        | bat buoc, 2-255 ky tu                                                          |
| `email`           | bat buoc, format email, max 255                                                |
| `password`        | bat buoc, 8-100 ky tu, co it nhat 1 chu HOA + 1 chu thuong + 1 chu so          |
| `isTermsAccepted` | bat buoc = `true`                                                              |

> Validation fail tra ve **400** voi `errors` dictionary (xem muc 0.1).

**Response 200**

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "user": {
    "id": 1,
    "fullName": "Nguyen Van A",
    "email": "test@example.com",
    "avatarUrl": null,
    "authProvider": "local",
    "isActive": true,
    "createdAt": "2026-04-29T11:29:52.5714469Z"
  }
}
```

**Response 409** (email da ton tai)

```json
{
  "title": "Xung dot du lieu",
  "status": 409,
  "detail": "Email da duoc su dung",
  "instance": "/api/auth/register",
  "errorType": "conflict",
  "traceId": "..."
}
```

---

### 2.2 `POST /api/auth/login`

Dang nhap bang email + password. Tra ve JWT giong nhu register.

**Request body**

```json
{
  "email": "test@example.com",
  "password": "Password123"
}
```

**Curl**

```bash
curl -X POST http://localhost:5292/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Password123"}'
```

**Response 200** — giong nhu register response.

**Response 401** (sai email/password hoac tai khoan bi vo hieu hoa)

```json
{
  "title": "Chua xac thuc",
  "status": 401,
  "detail": "Email hoac mat khau khong dung",
  "instance": "/api/auth/login",
  "errorType": "unauthorized",
  "traceId": "..."
}
```

---

### 2.3 `GET /api/auth/me`

Lay thong tin user tu JWT (claim `sub` = user id). **Yeu cau Bearer token**.

**Curl** (script bash de luu token vao bien)

```bash
TOKEN=$(curl -s -X POST http://localhost:5292/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Password123"}' \
  | grep -oE '"accessToken":"[^"]+"' | sed 's/"accessToken":"//;s/"$//')

curl http://localhost:5292/api/auth/me -H "Authorization: Bearer $TOKEN"
```

**Response 200**

```json
{
  "id": 1,
  "fullName": "Nguyen Van A",
  "email": "test@example.com",
  "avatarUrl": null,
  "authProvider": "local",
  "isActive": true,
  "createdAt": "2026-04-29T11:29:53"
}
```

**Response 401** — Khong gui token, token sai, hoac token het han.

**Response 404** — Token hop le nhung user da bi xoa khoi DB.

```json
{
  "title": "Khong tim thay",
  "status": 404,
  "detail": "Khong tim thay nguoi dung voi id = 1",
  "instance": "/api/auth/me",
  "errorType": "not_found",
  "traceId": "..."
}
```

---

## 3. JWT Token Reference

### Claims trong access token

| Claim           | Y nghia                       |
| --------------- | ----------------------------- |
| `sub`           | User id (string)              |
| `email`         | Email user                    |
| `name`          | Ho ten                        |
| `jti`           | Token id (GUID)               |
| `auth_provider` | local / google / apple        |
| `role`          | `user` hoac `admin` - `[Authorize(Roles="admin")]` doc claim nay |
| `iss`           | `PharmaIntelAI`               |
| `aud`           | `PharmaIntelAI`               |
| `nbf` / `exp`   | Not before / expire timestamp |

### Decode token (test)

Mo Swagger UI hoac dan token vao https://jwt.io de xem payload.

### Doi thoi gian song

Sua `appsettings.json`:

```json
"Jwt": { "ExpireMinutes": 60 }
```

---

## 4. Categories (Danh muc thuoc)

> Cau truc co the tu tham chieu (parent/children). `slug` la duy nhat trong toan bang.

### 4.1 `GET /api/categories`

List co phan trang + filter.

**Query params**

| Param       | Default | Mo ta                                              |
| ----------- | ------- | -------------------------------------------------- |
| `q`         | -       | tim theo Name / Slug                               |
| `parentId`  | -       | loc theo cha (null = co cha bat ky / khong loc)    |
| `rootOnly`  | false   | true = chi danh muc cap 1 (parentId IS NULL)       |
| `isActive`  | -       | loc theo trang thai                                |
| `page`      | 1       | trang (1-based)                                    |
| `pageSize`  | 20      | toi da 100                                         |

**Curl**

```bash
curl "http://localhost:5292/api/categories?rootOnly=true&pageSize=10"
```

**Response 200**

```json
{
  "items": [
    {
      "id": 1, "parentId": null, "parentName": null,
      "name": "Thuoc cam cum", "slug": "thuoc-cam-cum",
      "icon": null, "displayOrder": 1, "isActive": true,
      "medicationCount": 1,
      "createdAt": "...", "updatedAt": "..."
    }
  ],
  "page": 1, "pageSize": 10, "totalCount": 1, "totalPages": 1,
  "hasNext": false, "hasPrevious": false
}
```

---

### 4.2 `GET /api/categories/tree`

Tra ve cay danh muc (tat ca cap, lam san sn cho FE menu).

**Query**: `?includeInactive=true|false` (default false)

```bash
curl "http://localhost:5292/api/categories/tree"
```

**Response 200**

```json
[
  {
    "id": 1, "name": "Thuoc cam cum", "slug": "thuoc-cam-cum",
    "icon": null, "displayOrder": 1, "isActive": true,
    "children": [
      { "id": 2, "name": "Thuoc ho", "slug": "thuoc-ho",
        "children": [] }
    ]
  }
]
```

---

### 4.3 `GET /api/categories/{id}`

```bash
curl http://localhost:5292/api/categories/1
```

**404** neu khong ton tai.

---

### 4.4 `POST /api/categories` (admin)

Tao danh muc moi. Neu khong gui `slug` -> tu sinh tu `name` (loai bo dau, lower-case, gach noi).

**Request body**

```json
{
  "name": "Thuoc cam cum",
  "slug": "thuoc-cam-cum",
  "parentId": null,
  "icon": "pill",
  "displayOrder": 1,
  "isActive": true
}
```

**Curl**

```bash
curl -X POST http://localhost:5292/api/categories \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name":"Thuoc cam cum","displayOrder":1}'
```

**Response 201** — body giong `GET /{id}`. Header `Location: /api/categories/{id}`.

**Loi thuong gap**

| Status | Khi nao                                    |
| ------ | ------------------------------------------ |
| 400    | `name` trong, `slug` sai format            |
| 401    | thieu token                                |
| 404    | `parentId` khong ton tai                   |
| 409    | `slug` trung                               |

---

### 4.5 `PUT /api/categories/{id}` (admin)

Cap nhat toan bo cac truong (slug bat buoc o update).

```json
{
  "name": "Thuoc cam cum",
  "slug": "thuoc-cam-cum",
  "parentId": null,
  "icon": "pill",
  "displayOrder": 2,
  "isActive": true
}
```

**Loi**: 400 / 401 / 404 / 409. Ngoai ra **400** khi `parentId == id` hoac tao vong lap (parent chain quay lai chinh no).

---

### 4.6 `DELETE /api/categories/{id}` (admin)

```bash
curl -X DELETE http://localhost:5292/api/categories/2 -H "Authorization: Bearer $TOKEN"
```

**Response 204** khi thanh cong.
**409** neu danh muc co Children hoac Medications dang tham chieu:

```json
{
  "title": "Xung dot du lieu", "status": 409,
  "detail": "Khong the xoa - danh muc co 1 danh muc con",
  "errorType": "conflict"
}
```

---

## 5. Medications (Thuoc)

### 5.1 `GET /api/medications`

List co phan trang + filter + sort.

**Query params**

| Param                    | Default | Mo ta                                        |
| ------------------------ | ------- | -------------------------------------------- |
| `q`                      | -       | tim theo Name / Sku / GenericName            |
| `categoryId`             | -       | loc theo danh muc                            |
| `isActive`               | -       | true / false                                 |
| `isFeatured`             | -       |                                              |
| `isBestSeller`           | -       |                                              |
| `isPrescriptionRequired` | -       |                                              |
| `minPrice` / `maxPrice`  | -       | khoang gia                                   |
| `sortBy`                 | name    | `name` \| `price` \| `createdAt`             |
| `sortDesc`               | false   | true = sap xep giam dan                      |
| `page` / `pageSize`      | 1 / 20  |                                              |

**Curl**

```bash
curl "http://localhost:5292/api/medications?categoryId=1&sortBy=price&sortDesc=true"
```

**Response 200**

```json
{
  "items": [
    {
      "id": 1, "sku": "PARA-500", "name": "Paracetamol 500mg",
      "genericName": "Paracetamol", "manufacturer": "DHG Pharma",
      "price": 12000.00, "discountPercent": 10.00, "finalPrice": 10800.00,
      "categoryId": 1, "categoryName": "Thuoc cam cum",
      "imageUrl": null, "isFeatured": true, "isBestSeller": false,
      "isPrescriptionRequired": false, "stockQuantity": 100, "isActive": true
    }
  ],
  "page": 1, "pageSize": 20, "totalCount": 1
}
```

> `finalPrice` la field tinh toan (`price * (1 - discountPercent/100)`), khong gui khi POST/PUT.

---

### 5.2 `GET /api/medications/{id}`

Tra ve full detail (bao gom mo ta, huong dan, tac dung phu...).

```bash
curl http://localhost:5292/api/medications/1
```

**404** neu khong ton tai.

---

### 5.3 `POST /api/medications` (admin)

**Request body** (toan bo truong)

```json
{
  "sku": "PARA-500",
  "name": "Paracetamol 500mg",
  "genericName": "Paracetamol",
  "manufacturer": "DHG Pharma",
  "registrationNumber": "VN-12345-22",
  "description": "Thuoc giam dau, ha sot",
  "dosage": "500mg",
  "packaging": "Hop 10 vi x 10 vien",
  "price": 12000,
  "discountPercent": 10,
  "categoryId": 1,
  "usageInstructions": "Uong sau an, 1 vien moi 4-6 tieng",
  "benefits": "Giam dau dau, ha sot, dau co",
  "activeIngredients": "Paracetamol",
  "contraindications": "Suy gan nang",
  "sideEffects": "Buon non, di ung",
  "storageInstructions": "Noi kho thoang, duoi 30 do C",
  "imageUrl": "https://example.com/para.jpg",
  "isFeatured": true,
  "isBestSeller": false,
  "isPrescriptionRequired": false,
  "stockQuantity": 100,
  "isActive": true
}
```

**Validation** (`MedicationFieldsValidator`)

| Field             | Rule                                            |
| ----------------- | ----------------------------------------------- |
| `sku`             | bat buoc, max 100                               |
| `name`            | bat buoc, max 255                               |
| `price`           | >= 0                                            |
| `discountPercent` | 0 - 100                                         |
| `stockQuantity`   | >= 0                                            |
| `categoryId`      | > 0 (va phai ton tai trong DB)                  |

**Loi**

| Status | Khi nao                                  |
| ------ | ---------------------------------------- |
| 400    | validation                               |
| 401    | thieu token                              |
| 404    | `categoryId` khong ton tai               |
| 409    | `sku` trung                              |

**Response 201** — body giong `GET /{id}`. Header `Location: /api/medications/{id}`.

---

### 5.4 `PUT /api/medications/{id}` (admin)

Cau truc body giong `POST`. Update toan bo (replace).

---

### 5.5 `DELETE /api/medications/{id}` (admin)

```bash
curl -X DELETE http://localhost:5292/api/medications/1 -H "Authorization: Bearer $TOKEN"
```

**Response 204** khi thanh cong.
**409** neu thuoc dang ton tai trong CartItem / OrderItem / PrescriptionItem / DiagnosticResultMedication.

---

## 6. Cart (Gio hang)

> **Tat ca endpoint deu yeu cau JWT.** UserId duoc lay tu claim `sub` cua token,
> client khong can truyen userId. Moi user thay duoc gio hang cua chinh minh.

### Quy tac nghiep vu

- Cap `(userId, medicationId)` la duy nhat trong bang `cart_items`.
- ADD cung medication 2 lan -> **cong don** quantity (khong tao 2 row).
- Quantity moi (cu + moi) khong duoc vuot `medication.StockQuantity` -> **409**.
- Quantity toi da 999 / san pham.
- Medication phai `IsActive = true` thi moi them moi duoc -> **409** neu inactive.
- Item da co trong cart van xem duoc khi thuoc disable hoac ton kho < quantity,
  nhung `isAvailable = false` (FE chan checkout).

---

### 6.1 `GET /api/cart`

Lay gio hang cua user dang dang nhap.

```bash
curl http://localhost:5292/api/cart -H "Authorization: Bearer $TOKEN"
```

**Response 200**

```json
{
  "items": [
    {
      "id": 1, "medicationId": 1,
      "sku": "PARA-500", "name": "Paracetamol 500mg",
      "imageUrl": null, "manufacturer": "DHG Pharma",
      "unitPrice": 15000.00, "discountPercent": 5.00,
      "finalUnitPrice": 14250.00,
      "quantity": 2, "lineTotal": 28500.00,
      "isPrescriptionRequired": false,
      "stockQuantity": 80, "isAvailable": true,
      "addedAt": "2026-04-29T17:59:02"
    }
  ],
  "totalItems": 2,
  "distinctItems": 1,
  "subtotal": 30000.00,
  "totalDiscount": 1500.00,
  "total": 28500.00,
  "hasUnavailableItems": false,
  "hasPrescriptionRequired": false
}
```

> Cart trong tra ve mang `items: []` voi total = 0 (khong phai 404).

---

### 6.2 `POST /api/cart/items`

Them thuoc vao gio. Neu da co -> cong don quantity.

**Request body**

```json
{ "medicationId": 1, "quantity": 2 }
```

**Curl**

```bash
curl -X POST http://localhost:5292/api/cart/items \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"medicationId":1,"quantity":2}'
```

**Validation**: `medicationId > 0`, `quantity` 1-999.

**Response 200**: trả full `CartDto` (giong GET).

**Loi**

| Status | Khi nao                                                       |
| ------ | ------------------------------------------------------------- |
| 400    | quantity <= 0, medicationId <= 0                              |
| 401    | thieu token                                                   |
| 404    | medication khong ton tai                                      |
| 409    | medication inactive / vuot ton kho / vuot 999                 |

---

### 6.3 `PUT /api/cart/items/{medicationId}`

Set quantity tuyet doi (khong cong don).

```json
{ "quantity": 5 }
```

```bash
curl -X PUT http://localhost:5292/api/cart/items/1 \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"quantity":5}'
```

**Loi**: 400 / 401 / 404 (item chua co trong gio) / 409 (vuot ton kho / inactive).

---

### 6.4 `DELETE /api/cart/items/{medicationId}`

Xoa 1 thuoc khoi gio.

```bash
curl -X DELETE http://localhost:5292/api/cart/items/1 \
  -H "Authorization: Bearer $TOKEN"
```

**Response 200** voi cart con lai. **404** neu item khong co trong gio.

---

### 6.5 `DELETE /api/cart`

Xoa toan bo gio hang.

```bash
curl -X DELETE http://localhost:5292/api/cart \
  -H "Authorization: Bearer $TOKEN"
```

**Response 204** (No Content).

---

## 7. Orders / Checkout

> **Tat ca endpoint deu yeu cau JWT.** Order chi visible voi owner (ngay ca khi co orderId).

### Quy tac nghiep vu

- Cart phai khong rong moi checkout duoc (409).
- `Address` va `PaymentMethod` phai thuoc ve user dang dang nhap (403).
- Moi medication trong cart phai `IsActive` va du `StockQuantity` (409).
- **Snapshot tai thoi diem checkout**: `MedicationNameSnapshot`, `UnitPrice`, `DiscountPercent` o `OrderItem`; `ShippingRecipientName/Phone/FullAddress`, `PaymentTypeSnapshot` o `Order`.
- **OrderCode** dang `ORD-yyyyMMdd-XXXXXX`, duy nhat (retry 5 lan neu trung).
- **Initial state**: `status="pending"`, `paymentStatus="cod_pending"` voi COD, `"unpaid"` voi cac type khac.
- **Stock decrement** trong cung 1 transaction voi insert order/items + clear cart.
- Phase nay user **chi cancel duoc don o status `pending`**. Khi cancel: stock duoc restore.
- **ShippingFee** hien co dinh 30,000 VND.

---

### 7.1 `POST /api/orders/checkout`

Bien cart hien tai thanh order.

**Request body**

```json
{ "addressId": 1, "paymentMethodId": 1 }
```

**Curl**

```bash
curl -X POST http://localhost:5292/api/orders/checkout \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"addressId":1,"paymentMethodId":1}'
```

**Response 201**

```json
{
  "id": 1,
  "orderCode": "ORD-20260429-8MN67H",
  "addressId": 1, "paymentMethodId": 1,
  "subtotal": 73500.00, "shippingFee": 30000.00, "total": 103500.00,
  "status": "pending", "paymentStatus": "cod_pending",
  "paymentTypeSnapshot": "cod",
  "shippingRecipientName": "Nguyen Van A",
  "shippingPhone": "0901234567",
  "shippingFullAddress": "So 1 Pham Van Bach, Dich Vong, Cau Giay, Ha Noi",
  "itemCount": 2,
  "createdAt": "...", "updatedAt": "...",
  "items": [
    {
      "id": 1, "medicationId": 1,
      "medicationName": "Paracetamol 500mg",
      "quantity": 2, "unitPrice": 15000.00,
      "discountPercent": 5.00, "totalPrice": 28500.00
    }
  ]
}
```

**Loi**

| Status | Khi nao                                                            |
| ------ | ------------------------------------------------------------------ |
| 400    | validation (addressId/paymentMethodId <= 0)                        |
| 401    | thieu token                                                        |
| 403    | address/payment method khong thuoc user                            |
| 404    | address/payment method khong ton tai                               |
| 409    | cart trong, medication ngung kinh doanh, het hang                  |

---

### 7.2 `GET /api/orders/my`

Liet ke don cua user dang dang nhap (newest first), co paging va filter.

**Query**: `?status=pending&paymentStatus=unpaid&page=1&pageSize=20`

```bash
curl http://localhost:5292/api/orders/my -H "Authorization: Bearer $TOKEN"
```

**Response 200**: `PagedResult<OrderListItemDto>`.

---

### 7.3 `GET /api/orders/{id}`

Chi tiet don, **chi owner xem duoc** (403 cho user khac).

```bash
curl http://localhost:5292/api/orders/1 -H "Authorization: Bearer $TOKEN"
```

| Status | Khi nao              |
| ------ | -------------------- |
| 200    | OK + full DTO        |
| 401    | thieu token          |
| 403    | don khong cua user   |
| 404    | don khong ton tai    |

---

### 7.4 `POST /api/orders/{id}/cancel` (user)

User huy don cua chinh minh khi don dang `pending`. Token user thuong la du.

**Curl**

```bash
curl -X POST http://localhost:5292/api/orders/1/cancel \
  -H "Authorization: Bearer $TOKEN"
```

**Effect**:
- Order `status` -> `cancelled`
- Neu `paymentStatus="paid"` -> chuyen sang `refunded`
- **Restore stock** cho moi item

| Status | Khi nao                                                |
| ------ | ------------------------------------------------------ |
| 200    | OK                                                     |
| 401    | thieu token                                            |
| 403    | don khong cua user                                     |
| 404    | don khong ton tai                                      |
| 409    | don khong o trang thai `pending`                       |

---

### 7.5 `PUT /api/orders/{id}/status` (admin)

**Yeu cau role `admin`** (`[Authorize(Roles = "admin")]`). Admin chuyen status theo state machine:

```
pending    -> confirmed | cancelled
confirmed  -> processing | cancelled
processing -> shipping  | cancelled
shipping   -> delivered | cancelled
delivered  -> refunded
cancelled / refunded -> (terminal)
```

**Tu dong cap nhat `paymentStatus`**:
- `cancelled` voi `paid` -> `refunded`
- `delivered` voi COD -> `paid`
- Cac case khac giu nguyen.

**Restore stock** khi chuyen sang `cancelled` (lan dau).

**Request body**

```json
{ "status": "confirmed" }
```

**Curl**

```bash
curl -X PUT http://localhost:5292/api/orders/1/status \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"status":"confirmed"}'
```

| Status | Khi nao                                                |
| ------ | ------------------------------------------------------ |
| 200    | OK                                                     |
| 400    | status ngoai allowed list                              |
| 401    | thieu token                                            |
| 403    | token khong co role `admin`                            |
| 404    | don khong ton tai                                      |
| 409    | transition khong hop le theo state machine             |

---

## 8. Addresses (Dia chi giao hang)

> **Tat ca endpoint deu yeu cau JWT.** Address la user-scoped: user khac truy cap -> 403.

### Quy tac nghiep vu

- Moi user chi co **1 dia chi mac dinh** (`isDefault=true`). Set default cho dia chi moi -> tu dong reset cac dia chi khac ve `false`.
- Dia chi **dau tien** tao ra cua user **mac dinh la default** (du request gui `isDefault=false`).
- **Khong xoa cung neu da co order tham chieu** -> soft delete (`isActive=false`); chua co order -> xoa cung.
- Khi xoa dia chi dang la default va con dia chi khac active -> **tu dong promote dia chi moi nhat lam default**.
- `phone` validate theo regex VN: `^(0|\+84)\d{9,10}$`.
- `province` / `district` / `ward` / `streetAddress` la string thuan, max length 100/100/100/500.

---

### 8.1 `GET /api/addresses/my`

Liet ke dia chi cua user dang dang nhap (default truoc, sau do moi nhat truoc), co paging va filter.

**Query**: `?q=hanoi&isActive=true&page=1&pageSize=20`
- `q`: tim theo `recipientName` hoac `streetAddress` (case-insensitive).
- `isActive`: `true|false`, bo qua de lay tat ca.

**Curl**

```bash
curl "http://localhost:5292/api/addresses/my?page=1&pageSize=20" \
  -H "Authorization: Bearer $TOKEN"
```

**Response 200**: `PagedResult<AddressDto>`.

```json
{
  "items": [
    {
      "id": 1,
      "userId": 1,
      "recipientName": "Nguyen Van A",
      "phone": "0901234567",
      "province": "Ha Noi",
      "district": "Cau Giay",
      "ward": "Dich Vong",
      "streetAddress": "So 1 Pham Van Bach",
      "fullAddress": "So 1 Pham Van Bach, Dich Vong, Cau Giay, Ha Noi",
      "isDefault": true,
      "isActive": true,
      "createdAt": "2026-04-30T10:00:00",
      "updatedAt": "2026-04-30T10:00:00"
    }
  ],
  "page": 1, "pageSize": 20, "totalCount": 1,
  "totalPages": 1, "hasNext": false, "hasPrevious": false
}
```

---

### 8.2 `GET /api/addresses/{id}`

Chi tiet dia chi, **chi owner xem duoc** (403 cho user khac).

```bash
curl http://localhost:5292/api/addresses/1 -H "Authorization: Bearer $TOKEN"
```

| Status | Khi nao              |
| ------ | -------------------- |
| 200    | OK + `AddressDto`    |
| 401    | thieu token          |
| 403    | dia chi cua user khac|
| 404    | dia chi khong ton tai|

---

### 8.3 `POST /api/addresses`

Tao dia chi moi. Neu `isDefault=true` (hoac day la dia chi dau tien) -> reset default cu va set new lam default.

**Request body**

```json
{
  "recipientName": "Nguyen Van A",
  "phone": "0901234567",
  "province": "Ha Noi",
  "district": "Cau Giay",
  "ward": "Dich Vong",
  "streetAddress": "So 1 Pham Van Bach",
  "isDefault": true
}
```

**Curl**

```bash
curl -X POST http://localhost:5292/api/addresses \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"recipientName":"Nguyen Van A","phone":"0901234567","province":"Ha Noi","district":"Cau Giay","ward":"Dich Vong","streetAddress":"So 1 Pham Van Bach","isDefault":true}'
```

**Response 201** (header `Location: /api/addresses/{id}`): `AddressDto`.

**Loi**

| Status | Khi nao                                                        |
| ------ | -------------------------------------------------------------- |
| 400    | validation (phone sai format, recipientName < 2 ky tu, ...)    |
| 401    | thieu token                                                    |

---

### 8.4 `PUT /api/addresses/{id}`

Cap nhat thong tin dia chi (full update). Khong doi `isDefault` qua endpoint nay - dung 8.6.

**Request body**

```json
{
  "recipientName": "Nguyen Van B",
  "phone": "+84909123456",
  "province": "TP Ho Chi Minh",
  "district": "Quan 1",
  "ward": "Ben Nghe",
  "streetAddress": "12 Le Loi",
  "isActive": true
}
```

**Curl**

```bash
curl -X PUT http://localhost:5292/api/addresses/1 \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"recipientName":"Nguyen Van B","phone":"+84909123456","province":"TP Ho Chi Minh","district":"Quan 1","ward":"Ben Nghe","streetAddress":"12 Le Loi","isActive":true}'
```

**Response 200**: `AddressDto` (sau cap nhat).

| Status | Khi nao                       |
| ------ | ----------------------------- |
| 200    | OK                            |
| 400    | validation                    |
| 401    | thieu token                   |
| 403    | dia chi cua user khac         |
| 404    | dia chi khong ton tai         |

---

### 8.5 `DELETE /api/addresses/{id}`

Xoa dia chi.
- **Chua co order tham chieu** -> xoa cung khoi DB.
- **Da co order tham chieu** -> soft delete (`isActive=false`, `isDefault=false`) de giu lich su.
- Neu dia chi bi xoa la default va user con dia chi active khac -> tu dong promote dia chi moi nhat lam default.

```bash
curl -X DELETE http://localhost:5292/api/addresses/1 -H "Authorization: Bearer $TOKEN"
```

**Response 204** (No Content).

| Status | Khi nao                       |
| ------ | ----------------------------- |
| 204    | OK (xoa cung hoac soft delete)|
| 401    | thieu token                   |
| 403    | dia chi cua user khac         |
| 404    | dia chi khong ton tai         |

---

### 8.6 `PUT /api/addresses/{id}/default`

Dat dia chi nay lam default cua user. Reset cac dia chi khac ve `isDefault=false` trong cung transaction.

```bash
curl -X PUT http://localhost:5292/api/addresses/2/default \
  -H "Authorization: Bearer $TOKEN"
```

**Response 200**: `AddressDto` (voi `isDefault=true`).

| Status | Khi nao                                  |
| ------ | ---------------------------------------- |
| 200    | OK                                       |
| 401    | thieu token                              |
| 403    | dia chi cua user khac                    |
| 404    | dia chi khong ton tai                    |
| 409    | dia chi `isActive=false` -> khong cho set|

---

## 9. Users (Profile + Settings)

> **Tat ca endpoint deu yeu cau JWT.** User chi xem/sua profile va settings cua chinh minh.

### Quy tac nghiep vu

- **Email IMMUTABLE**: khong co endpoint update email. Email duoc gan luc dang ky, khong doi qua API.
- **JWT claim `name` se stale** sau khi update FullName. User can dang nhap lai de token chua FullName moi (token cu van valid den khi het han).
- **ChangePassword**: bat buoc cung cap `currentPassword` va xac nhan; kiem tra qua `IPasswordHasher<User>` cua ASP.NET Core Identity.
- **Password rules** (giong Register): >= 8 ky tu, co chu HOA + thuong + so, max 100 ky tu, NewPassword phai khac CurrentPassword.
- **Settings 1:1 lazy provisioning**: `RegisterAsync` khong tao `UserSetting`, GET/PUT lan dau **tu dong tao record default** (`darkMode="system"`, `languageCode="vi"`, notification/sound = true).
- **DarkMode** chi nhan: `light`, `dark`, `system` (CHECK constraint o DB). **LanguageCode** support: `vi`, `en`.
- Tai khoan OAuth (`authProvider != "local"`, khong co `passwordHash`) goi change-password -> 409.

---

### 9.1 `GET /api/users/me`

Xem profile cua user dang dang nhap.

**Curl**

```bash
curl http://localhost:5292/api/users/me -H "Authorization: Bearer $TOKEN"
```

**Response 200**

```json
{
  "id": 1,
  "fullName": "Nguyen Van A",
  "email": "test@example.com",
  "avatarUrl": null,
  "authProvider": "local",
  "isActive": true,
  "createdAt": "2026-04-29T11:29:53",
  "updatedAt": "2026-04-30T08:15:00"
}
```

> **Khac voi `GET /api/auth/me`**: endpoint nay tra them `updatedAt`, dung khi can hien thi phan profile chi tiet. `auth/me` van con dung cho check token nhanh.

| Status | Khi nao              |
| ------ | -------------------- |
| 200    | OK                   |
| 401    | thieu/sai token      |
| 404    | user da bi xoa khoi DB|

---

### 9.2 `PUT /api/users/me`

Cap nhat thong tin profile (chi `fullName` va `avatarUrl`). Email khong cho doi.

**Request body**

```json
{
  "fullName": "Nguyen Van B",
  "avatarUrl": "https://cdn.example.com/avatars/1.jpg"
}
```

`avatarUrl` co the gui `null` hoac chuoi rong de xoa avatar.

**Curl**

```bash
curl -X PUT http://localhost:5292/api/users/me \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"fullName":"Nguyen Van B","avatarUrl":"https://cdn.example.com/avatars/1.jpg"}'
```

**Response 200**: `UserProfileDto` sau cap nhat (`updatedAt` da refresh).

**Loi**

| Status | Khi nao                                                              |
| ------ | -------------------------------------------------------------------- |
| 400    | validation (fullName < 2 ky tu, avatarUrl khong phai http/https URL) |
| 401    | thieu token                                                          |
| 404    | user da bi xoa khoi DB                                               |

> **Luu y**: token JWT dang giu **van con FullName cu** - FE nen cap nhat state local hoac cho user dang nhap lai de refresh token.

---

### 9.3 `PUT /api/users/me/change-password`

Doi mat khau. Bat buoc cung cap `currentPassword`; `confirmPassword` phai trung `newPassword`.

**Request body**

```json
{
  "currentPassword": "Password123",
  "newPassword": "NewPassword456",
  "confirmPassword": "NewPassword456"
}
```

**Curl**

```bash
curl -X PUT http://localhost:5292/api/users/me/change-password \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"currentPassword":"Password123","newPassword":"NewPassword456","confirmPassword":"NewPassword456"}'
```

**Response 204** (No Content). Token cu **van valid** den het han - design hien tai khong revoke session, FE co the chu dong logout sau khi doi mat khau.

**Loi**

| Status | Khi nao                                                                                |
| ------ | -------------------------------------------------------------------------------------- |
| 400    | currentPassword sai (`{"currentPassword": ["Mat khau hien tai khong dung"]}`)          |
| 400    | validation (password khong du do manh, confirmPassword khong khop, newPassword == cu)  |
| 401    | thieu token                                                                            |
| 404    | user da bi xoa khoi DB                                                                 |
| 409    | tai khoan OAuth (google/apple) khong co passwordHash de doi                            |

---

### 9.4 `GET /api/users/me/settings`

Xem settings ca nhan. Neu user **chua co settings** (sau khi Register), backend **tu dong tao record default** roi tra ve.

**Curl**

```bash
curl http://localhost:5292/api/users/me/settings -H "Authorization: Bearer $TOKEN"
```

**Response 200**

```json
{
  "darkMode": "system",
  "languageCode": "vi",
  "notificationEnabled": true,
  "reminderSoundEnabled": true,
  "createdAt": "2026-04-30T10:00:00",
  "updatedAt": "2026-04-30T10:00:00"
}
```

| Status | Khi nao              |
| ------ | -------------------- |
| 200    | OK (auto-create neu can)|
| 401    | thieu token          |
| 404    | user da bi xoa khoi DB|

---

### 9.5 `PUT /api/users/me/settings`

Cap nhat tat ca 4 truong settings (full update). Neu chua co record -> tu dong tao roi cap nhat.

**Request body**

```json
{
  "darkMode": "dark",
  "languageCode": "en",
  "notificationEnabled": false,
  "reminderSoundEnabled": true
}
```

**Curl**

```bash
curl -X PUT http://localhost:5292/api/users/me/settings \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"darkMode":"dark","languageCode":"en","notificationEnabled":false,"reminderSoundEnabled":true}'
```

**Response 200**: `UserSettingsDto` sau cap nhat.

**Loi**

| Status | Khi nao                                                       |
| ------ | ------------------------------------------------------------- |
| 400    | `darkMode` khong thuoc `light/dark/system`                    |
| 400    | `languageCode` khong thuoc `vi/en`                            |
| 401    | thieu token                                                   |
| 404    | user da bi xoa khoi DB                                        |

---

## 10. Prescriptions (Don thuoc)

> **Tat ca endpoint deu yeu cau JWT.** User chi xem/sua/xoa don thuoc cua chinh minh.

### Quy tac nghiep vu

- Don thuoc moi tao **luon o status `draft`** (du request gui status khac).
- **Items chi them/sua/xoa khi don thuoc dang `draft`**. Khi da `active`/`completed`/`cancelled` -> 409.
- **Status transition cho phep**:
  - `draft` -> `active` | `cancelled`
  - `active` -> `completed` | `cancelled`
  - `completed` / `cancelled` la **terminal** (khong chuyen tiep duoc).
  - **`expired`** la system-driven (theo `prescribedDate`), user khong tu set duoc.
- Chuyen `draft` -> `active` yeu cau **co it nhat 1 item**.
- **Delete chi cho phep khi chua co `Order` tham chieu** (`Order.PrescriptionId`). Da co order -> 409 (giu lich su).
- **DoctorId** neu cung cap: bac si phai ton tai + `IsActive`. He thong **auto snapshot** `Doctor.FullName` vao `DoctorNameSnapshot` neu user chua nhap thu cong.
- **MedicationId** neu cung cap: thuoc phai ton tai + `IsActive`. **Auto snapshot** `Medication.Name` vao `medicationName`. Neu khong co `MedicationId`, `medicationName` la **bat buoc** (cho phep nhap free-text tu don giay).
- `verificationStatus` (chi `not_required` mac dinh) thuoc qui trinh **pharmacist verify**, khong expose o API nay.

### Field reference

| Field                | Type        | Notes                                                           |
| -------------------- | ----------- | --------------------------------------------------------------- |
| `status`             | string      | `draft`, `active`, `completed`, `expired`, `cancelled`          |
| `verificationStatus` | string      | `not_required`, `pending`, `verified`, `rejected`               |
| `prescribedDate`     | `date`      | YYYY-MM-DD; khong duoc o tuong lai                              |
| `doctorNameSnapshot` | string?     | Snapshot luc tao - khong doi khi Doctor doi ten ve sau          |
| `medicationName`     | string      | Snapshot luc them - khong doi khi Medication doi ten ve sau     |

---

### 10.1 `GET /api/prescriptions/my`

Liet ke don thuoc cua user (newest first), co paging va filter.

**Query**: `?status=draft&q=cam%20cum&page=1&pageSize=20`
- `status`: filter theo status.
- `q`: tim theo `title` hoac `doctorNameSnapshot` (case-insensitive).

**Curl**

```bash
curl "http://localhost:5292/api/prescriptions/my?status=draft" \
  -H "Authorization: Bearer $TOKEN"
```

**Response 200**: `PagedResult<PrescriptionListItemDto>`.

```json
{
  "items": [
    {
      "id": 1,
      "doctorId": null,
      "doctorNameSnapshot": "BS Nguyen Van A",
      "title": "Don kham cam cum 2026-04-30",
      "prescribedDate": "2026-04-30",
      "status": "draft",
      "verificationStatus": "not_required",
      "itemCount": 0,
      "createdAt": "2026-04-30T10:00:00",
      "updatedAt": "2026-04-30T10:00:00"
    }
  ],
  "page": 1, "pageSize": 20, "totalCount": 1,
  "totalPages": 1, "hasNext": false, "hasPrevious": false
}
```

---

### 10.2 `GET /api/prescriptions/{id}`

Chi tiet don thuoc kem `items`. **Chi owner xem duoc** (403 cho user khac).

```bash
curl http://localhost:5292/api/prescriptions/1 -H "Authorization: Bearer $TOKEN"
```

**Response 200**

```json
{
  "id": 1,
  "doctorId": null,
  "doctorNameSnapshot": "BS Nguyen Van A",
  "title": "Don kham cam cum 2026-04-30",
  "prescribedDate": "2026-04-30",
  "status": "draft",
  "verificationStatus": "not_required",
  "itemCount": 2,
  "createdAt": "...",
  "updatedAt": "...",
  "items": [
    {
      "id": 10, "prescriptionId": 1,
      "medicationId": 5,
      "medicationName": "Paracetamol 500mg",
      "dosage": "500mg",
      "frequency": "3 lan/ngay",
      "duration": "5 ngay"
    },
    {
      "id": 11, "prescriptionId": 1,
      "medicationId": null,
      "medicationName": "Vitamin C tu nhan (free-text)",
      "dosage": "1 vien", "frequency": "moi ngay", "duration": "10 ngay"
    }
  ]
}
```

| Status | Khi nao              |
| ------ | -------------------- |
| 200    | OK + full DTO        |
| 401    | thieu token          |
| 403    | don khong cua user   |
| 404    | don khong ton tai    |

---

### 10.3 `POST /api/prescriptions`

Tao don thuoc moi (status mac dinh `draft`).

**Request body**

```json
{
  "doctorId": null,
  "doctorNameSnapshot": "BS Nguyen Van A",
  "title": "Don kham cam cum 2026-04-30",
  "prescribedDate": "2026-04-30"
}
```

Hoac chon doctor co san:

```json
{ "doctorId": 1, "title": "Tai kham", "prescribedDate": "2026-04-30" }
```

**Curl**

```bash
curl -X POST http://localhost:5292/api/prescriptions \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"doctorNameSnapshot":"BS Nguyen Van A","title":"Don kham cam cum","prescribedDate":"2026-04-30"}'
```

**Response 201**: `PrescriptionDto` (`Location` header tro toi 10.2). Status mac dinh `draft`, items rong.

| Status | Khi nao                                                  |
| ------ | -------------------------------------------------------- |
| 201    | OK                                                       |
| 400    | validation (prescribedDate tuong lai, title > 255, ...)  |
| 401    | thieu token                                              |
| 404    | doctorId duoc gui nhung khong ton tai                    |
| 409    | doctor `isActive=false`                                  |

---

### 10.4 `PUT /api/prescriptions/{id}`

Cap nhat header + chuyen status. **Lam toan bo cac field cua header**, khong patch tung phan.

**Request body**

```json
{
  "doctorId": null,
  "doctorNameSnapshot": "BS Tran Thi B",
  "title": "Don kham hen suyen",
  "prescribedDate": "2026-04-30",
  "status": "active"
}
```

**Status transitions cho phep**:
- `draft` -> `active` (yeu cau co >= 1 item)
- `draft` -> `cancelled`
- `active` -> `completed`
- `active` -> `cancelled`

**Curl**

```bash
curl -X PUT http://localhost:5292/api/prescriptions/1 \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"doctorNameSnapshot":"BS Tran Thi B","title":"Don kham hen suyen","prescribedDate":"2026-04-30","status":"active"}'
```

**Response 200**: `PrescriptionDto` sau cap nhat.

| Status | Khi nao                                                                        |
| ------ | ------------------------------------------------------------------------------ |
| 200    | OK                                                                             |
| 400    | validation (status ngoai allowed list, prescribedDate tuong lai, ...)          |
| 401    | thieu token                                                                    |
| 403    | don khong cua user                                                             |
| 404    | don khong ton tai / doctorId khong ton tai                                     |
| 409    | status transition khong hop le, hoac active hoa khi don rong, hoac doctor ngung|

---

### 10.5 `DELETE /api/prescriptions/{id}`

Xoa don thuoc. **Chi cho phep khi chua co Order tham chieu prescription nay.** Cascade delete `prescription_items`.

```bash
curl -X DELETE http://localhost:5292/api/prescriptions/1 -H "Authorization: Bearer $TOKEN"
```

**Response 204** (No Content).

| Status | Khi nao                                              |
| ------ | ---------------------------------------------------- |
| 204    | OK                                                   |
| 401    | thieu token                                          |
| 403    | don khong cua user                                   |
| 404    | don khong ton tai                                    |
| 409    | da co order tham chieu - giu lich su, khong cho xoa  |

---

### 10.6 `POST /api/prescriptions/{id}/items`

Them muc thuoc vao don. **Chi khi prescription dang `draft`.**

Co 2 cach:

**(a) Chon thuoc tu danh muc** (auto snapshot ten):

```json
{
  "medicationId": 5,
  "dosage": "500mg",
  "frequency": "3 lan/ngay",
  "duration": "5 ngay"
}
```

**(b) Free-text** (tu nhap khi thuoc khong co trong DB):

```json
{
  "medicationName": "Vitamin C tu nhan",
  "dosage": "1 vien",
  "frequency": "moi sang",
  "duration": "10 ngay"
}
```

**Response 201**: `PrescriptionItemDto`.

```json
{
  "id": 10, "prescriptionId": 1,
  "medicationId": 5,
  "medicationName": "Paracetamol 500mg",
  "dosage": "500mg", "frequency": "3 lan/ngay", "duration": "5 ngay"
}
```

| Status | Khi nao                                                                           |
| ------ | --------------------------------------------------------------------------------- |
| 201    | OK                                                                                |
| 400    | validation (medicationName rong khi khong co medicationId, do dai vuot gioi han)  |
| 401    | thieu token                                                                       |
| 403    | don khong cua user                                                                |
| 404    | don khong ton tai / medicationId khong ton tai                                    |
| 409    | don khong o trang thai `draft`, hoac thuoc da `isActive=false`                    |

---

### 10.7 `PUT /api/prescriptions/{id}/items/{itemId}`

Cap nhat muc thuoc. **Chi khi prescription dang `draft`.** Full update.

**Request body** (same as POST):

```json
{
  "medicationId": 5,
  "dosage": "1000mg",
  "frequency": "2 lan/ngay",
  "duration": "7 ngay"
}
```

**Curl**

```bash
curl -X PUT http://localhost:5292/api/prescriptions/1/items/10 \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"medicationId":5,"dosage":"1000mg","frequency":"2 lan/ngay","duration":"7 ngay"}'
```

**Response 200**: `PrescriptionItemDto`.

| Status | Khi nao                                                                |
| ------ | ---------------------------------------------------------------------- |
| 200    | OK                                                                     |
| 400    | validation                                                             |
| 401    | thieu token                                                            |
| 403    | don khong cua user                                                     |
| 404    | don/item khong ton tai, hoac item khong thuoc don nay                  |
| 409    | don khong o trang thai `draft`, hoac thuoc da `isActive=false`         |

---

### 10.8 `DELETE /api/prescriptions/{id}/items/{itemId}`

Xoa muc thuoc. **Chi khi prescription dang `draft`.**

```bash
curl -X DELETE http://localhost:5292/api/prescriptions/1/items/10 \
  -H "Authorization: Bearer $TOKEN"
```

**Response 204**.

| Status | Khi nao                                              |
| ------ | ---------------------------------------------------- |
| 204    | OK                                                   |
| 401    | thieu token                                          |
| 403    | don khong cua user                                   |
| 404    | don/item khong ton tai, hoac item khong thuoc don nay|
| 409    | don khong o trang thai `draft`                       |

---

## 11. Medication Reminders (Lich nhac thuoc)

> **Tat ca endpoint deu yeu cau JWT.** User chi xem/sua/xoa lich nhac cua chinh minh.

### Quan he &amp; flow

```
Prescription -> PrescriptionItem -> MedicationReminder -> MedicationReminderLog
                       (optional link)
```

User co the:
1. Tao lich nhac **link toi PrescriptionItem** (auto-snapshot ten thuoc), HOAC
2. Tao lich nhac **standalone** (free-text MedicationName, vi du vitamin tu mua).
3. Khi den gio nhac: client hien push notification.
4. User danh dau: `taken` (da uong) / `missed` (bo lo) / `skipped` (bo qua) -> ghi 1 log.
5. Xem lich su qua endpoint list logs (filter theo date range).

### Quy tac nghiep vu

- **Reminder status transitions** cho phep:
  - `active` <-> `paused`
  - `active` -> `completed` | `cancelled`
  - `paused` -> `completed` | `cancelled`
  - `completed` / `cancelled` la **terminal**.
- **Reminder moi tao luon o `active`** (request status bi bo qua).
- **PrescriptionItemId** neu cung cap: phai thuoc don thuoc cua chinh user (cross-tenant guard -> 403). Auto snapshot `MedicationName` tu PrescriptionItem neu user khong nhap.
- **Khong them log khi reminder dang `cancelled`** -> 409 (giu data sach).
- **Log POST chi cho `taken | missed | skipped`** (status `scheduled` la system-driven, khong expose).
- **CompletedAt** auto set theo status:
  - `taken`/`skipped` -> `UtcNow`
  - `missed` -> `null`
- **Delete reminder cascade xoa toan bo logs** (DB-enforced).

### Field reference

| Field           | Type     | Notes                                                      |
| --------------- | -------- | ---------------------------------------------------------- |
| `frequencyType` | string   | `once`, `daily`, `weekly`, `custom`                        |
| `reminderTime`  | TimeOnly | `HH:mm:ss` (gio nhac trong ngay, vi du `"08:00:00"`)       |
| `status`        | string   | `active`, `paused`, `completed`, `cancelled`               |
| Log `status`    | string   | `scheduled` (system) / `taken` / `missed` / `skipped`      |

---

### 11.1 `GET /api/medication-reminders`

Liet ke lich nhac cua user. Sap xep: `active` truoc, sau do theo `reminderTime`.

**Query**: `?status=active&q=paracetamol&page=1&pageSize=20`

**Curl**

```bash
curl "http://localhost:5292/api/medication-reminders?status=active" \
  -H "Authorization: Bearer $TOKEN"
```

**Response 200**: `PagedResult<MedicationReminderListItemDto>`.

```json
{
  "items": [
    {
      "id": 1,
      "prescriptionItemId": 10,
      "medicationName": "Paracetamol 500mg",
      "frequencyType": "daily",
      "reminderTime": "08:00:00",
      "status": "active",
      "logCount": 5,
      "createdAt": "...", "updatedAt": "..."
    }
  ],
  "page": 1, "pageSize": 20, "totalCount": 1,
  "totalPages": 1, "hasNext": false, "hasPrevious": false
}
```

---

### 11.2 `GET /api/medication-reminders/{id}`

Chi tiet lich nhac. **Chi owner xem duoc** (403 cho user khac).

```bash
curl http://localhost:5292/api/medication-reminders/1 -H "Authorization: Bearer $TOKEN"
```

| Status | Khi nao              |
| ------ | -------------------- |
| 200    | OK                   |
| 401    | thieu token          |
| 403    | lich nhac cua user khac|
| 404    | khong ton tai        |

---

### 11.3 `POST /api/medication-reminders`

Tao lich nhac moi (status mac dinh `active`).

**Request body** (link toi don thuoc):

```json
{
  "prescriptionItemId": 10,
  "frequencyType": "daily",
  "reminderTime": "08:00:00"
}
```

**Hoac standalone** (free-text):

```json
{
  "medicationName": "Vitamin C 1000mg",
  "frequencyType": "daily",
  "reminderTime": "07:30:00"
}
```

**Curl**

```bash
curl -X POST http://localhost:5292/api/medication-reminders \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"medicationName":"Vitamin C 1000mg","frequencyType":"daily","reminderTime":"07:30:00"}'
```

**Response 201**: `MedicationReminderDto` voi `Location` header tro toi 11.2.

| Status | Khi nao                                                              |
| ------ | -------------------------------------------------------------------- |
| 201    | OK                                                                   |
| 400    | validation (frequencyType ngoai allowed, medicationName rong khi standalone) |
| 401    | thieu token                                                          |
| 403    | prescriptionItemId thuoc don thuoc cua user khac                     |
| 404    | prescriptionItemId khong ton tai                                     |

---

### 11.4 `PUT /api/medication-reminders/{id}`

Cap nhat lich nhac + chuyen status. **Full update**.

**Request body**

```json
{
  "prescriptionItemId": 10,
  "frequencyType": "weekly",
  "reminderTime": "09:00:00",
  "status": "paused"
}
```

**Status transitions**: `active <-> paused`, `active|paused -> completed|cancelled`, `completed|cancelled` terminal.

**Response 200**: `MedicationReminderDto` sau cap nhat.

| Status | Khi nao                                                  |
| ------ | -------------------------------------------------------- |
| 200    | OK                                                       |
| 400    | validation                                               |
| 401    | thieu token                                              |
| 403    | lich nhac cua user khac / prescriptionItem cua user khac |
| 404    | khong ton tai                                            |
| 409    | status transition khong hop le                           |

---

### 11.5 `DELETE /api/medication-reminders/{id}`

Xoa lich nhac. **Cascade delete logs** (DB-enforced).

```bash
curl -X DELETE http://localhost:5292/api/medication-reminders/1 \
  -H "Authorization: Bearer $TOKEN"
```

**Response 204**.

| Status | Khi nao                |
| ------ | ---------------------- |
| 204    | OK                     |
| 401    | thieu token            |
| 403    | lich nhac cua user khac|
| 404    | khong ton tai          |

---

### 11.6 `POST /api/medication-reminders/{id}/logs`

Ghi nhan 1 lan nhac (user danh dau ket qua).

**Request body**

```json
{
  "scheduledAt": "2026-04-30T08:00:00Z",
  "status": "taken"
}
```

- `scheduledAt`: thoi diem dang ra phai uong (UTC).
- `status`: `taken` | `missed` | `skipped`.
- `completedAt` **server tu set**: `UtcNow` cho `taken`/`skipped`, `null` cho `missed`.

**Curl**

```bash
curl -X POST http://localhost:5292/api/medication-reminders/1/logs \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"scheduledAt":"2026-04-30T08:00:00Z","status":"taken"}'
```

**Response 201**: `MedicationReminderLogDto`.

```json
{
  "id": 100,
  "reminderId": 1,
  "scheduledAt": "2026-04-30T08:00:00",
  "completedAt": "2026-04-30T08:05:23",
  "status": "taken"
}
```

| Status | Khi nao                                                              |
| ------ | -------------------------------------------------------------------- |
| 201    | OK                                                                   |
| 400    | validation (status ngoai taken/missed/skipped, scheduledAt rong)     |
| 401    | thieu token                                                          |
| 403    | lich nhac cua user khac                                              |
| 404    | reminder khong ton tai                                               |
| 409    | reminder dang `cancelled` -> khong cho them log                      |

---

### 11.7 `GET /api/medication-reminders/{id}/logs`

Lich su log cua 1 reminder, paged.

**Query**: `?status=taken&fromDate=2026-04-01T00:00:00Z&toDate=2026-04-30T23:59:59Z&page=1&pageSize=20`

**Curl**

```bash
curl "http://localhost:5292/api/medication-reminders/1/logs?status=taken&page=1&pageSize=20" \
  -H "Authorization: Bearer $TOKEN"
```

**Response 200**: `PagedResult<MedicationReminderLogDto>`, sap xep `scheduledAt` giam dan.

| Status | Khi nao                  |
| ------ | ------------------------ |
| 200    | OK                       |
| 401    | thieu token              |
| 403    | reminder cua user khac   |
| 404    | reminder khong ton tai   |

---

## 12. Health Metrics (Chi so suc khoe)

> **Tat ca endpoint deu yeu cau JWT.** User chi xem/sua/xoa chi so cua chinh minh.

### Quy tac nghiep vu

- **6 loai chi so duoc CHECK constraint o DB**: `blood_pressure`, `heart_rate`, `temperature`, `weight`, `blood_sugar`, `oxygen_saturation`.
- **Voi `blood_pressure`**: `valueNumber` = tam thu (systolic), `valueNumber2` = tam truong (diastolic) - **bat buoc**. Cac loai khac: `valueNumber2` se bi server clear ve `null` (du request gui).
- **Range validate y khoa**:

  | metricType          | valueNumber range | valueNumber2 range | unit mac dinh |
  | ------------------- | ----------------- | ------------------ | ------------- |
  | `blood_pressure`    | 50 - 300 (systolic)| 30 - 200 (diastolic)| `mmHg`       |
  | `heart_rate`        | 30 - 250          | -                  | `bpm`         |
  | `temperature`       | 30 - 45           | -                  | `C`           |
  | `weight`            | 0.5 - 500         | -                  | `kg`          |
  | `blood_sugar`       | 1 - 50            | -                  | `mmol/L`      |
  | `oxygen_saturation` | 50 - 100          | -                  | `%`           |

- **`unit` auto-fill** theo metricType neu user khong nhap.
- **`recordedAt`** optional. Default = `UtcNow`. **Khong duoc tuong lai** (cho phep +5 phut de tranh clock skew).
- **Index `(user_id, metric_type, recorded_at)`** -> filter list theo type + date range cuc nhanh, phu hop cho bieu do.
- Cascade delete: xoa user -> tat ca chi so xoa theo (DB-enforced).

---

### 12.1 `GET /api/health-metrics`

Liet ke chi so cua user (newest first), co paging va filter.

**Query**: `?metricType=blood_pressure&fromDate=2026-04-01T00:00:00Z&toDate=2026-04-30T23:59:59Z&page=1&pageSize=20`

**Curl**

```bash
curl "http://localhost:5292/api/health-metrics?metricType=blood_pressure" \
  -H "Authorization: Bearer $TOKEN"
```

**Response 200**: `PagedResult<HealthMetricDto>`.

```json
{
  "items": [
    {
      "id": 1,
      "metricType": "blood_pressure",
      "valueNumber": 120.0,
      "valueNumber2": 80.0,
      "unit": "mmHg",
      "notes": "Sau khi ngu day",
      "recordedAt": "2026-04-30T07:30:00"
    }
  ],
  "page": 1, "pageSize": 20, "totalCount": 1,
  "totalPages": 1, "hasNext": false, "hasPrevious": false
}
```

---

### 12.2 `GET /api/health-metrics/{id}`

Chi tiet chi so. **Chi owner xem duoc** (403 cho user khac).

```bash
curl http://localhost:5292/api/health-metrics/1 -H "Authorization: Bearer $TOKEN"
```

| Status | Khi nao              |
| ------ | -------------------- |
| 200    | OK                   |
| 401    | thieu token          |
| 403    | chi so cua user khac |
| 404    | khong ton tai        |

---

### 12.3 `POST /api/health-metrics`

Ghi chi so moi.

**Vi du blood_pressure**:

```json
{
  "metricType": "blood_pressure",
  "valueNumber": 120,
  "valueNumber2": 80,
  "notes": "Sau khi ngu day",
  "recordedAt": "2026-04-30T07:30:00Z"
}
```

**Vi du heart_rate** (1 gia tri, unit auto-fill):

```json
{ "metricType": "heart_rate", "valueNumber": 72 }
```

**Vi du weight**:

```json
{ "metricType": "weight", "valueNumber": 65.5 }
```

**Curl**

```bash
curl -X POST http://localhost:5292/api/health-metrics \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"metricType":"blood_pressure","valueNumber":120,"valueNumber2":80,"notes":"Sau khi ngu day"}'
```

**Response 201**: `HealthMetricDto` voi `unit` da auto-fill (`mmHg`), `recordedAt` = UtcNow neu khong gui.

**Loi**

| Status | Khi nao                                                                        |
| ------ | ------------------------------------------------------------------------------ |
| 201    | OK                                                                             |
| 400    | metricType ngoai allowed list                                                  |
| 400    | valueNumber ngoai range cua metricType (vi du heart_rate=400)                  |
| 400    | blood_pressure thieu valueNumber2, hoac valueNumber2 ngoai 30-200              |
| 400    | recordedAt o tuong lai                                                         |
| 401    | thieu token                                                                    |

---

### 12.4 `PUT /api/health-metrics/{id}`

Cap nhat chi so. **Full update**. Co the doi metricType - server se tu clear `valueNumber2` neu metricType moi khong phai blood_pressure.

**Request body** (giong POST):

```json
{
  "metricType": "blood_pressure",
  "valueNumber": 125,
  "valueNumber2": 82,
  "notes": "Sau buoi tap"
}
```

**Curl**

```bash
curl -X PUT http://localhost:5292/api/health-metrics/1 \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"metricType":"blood_pressure","valueNumber":125,"valueNumber2":82}'
```

**Response 200**: `HealthMetricDto` sau cap nhat.

| Status | Khi nao                       |
| ------ | ----------------------------- |
| 200    | OK                            |
| 400    | validation                    |
| 401    | thieu token                   |
| 403    | chi so cua user khac          |
| 404    | khong ton tai                 |

---

### 12.5 `DELETE /api/health-metrics/{id}`

Xoa chi so.

```bash
curl -X DELETE http://localhost:5292/api/health-metrics/1 -H "Authorization: Bearer $TOKEN"
```

**Response 204**.

| Status | Khi nao                       |
| ------ | ----------------------------- |
| 204    | OK                            |
| 401    | thieu token                   |
| 403    | chi so cua user khac          |
| 404    | khong ton tai                 |

---

## 13. Notifications (Thong bao)

> **Tat ca endpoint deu yeu cau JWT.** User chi xem/danh dau/xoa thong bao cua chinh minh.

### Quy tac nghiep vu

- **Khong co POST**: notification do **system** tao thong qua event noi bo (order_update, reminder, prescription_new, ...). User chi tieu thu (read).
- **Cac `notificationType` quy uoc** (string thuan, khong CHECK constraint o DB):
  - `order_update` - cap nhat trang thai don hang
  - `reminder` - nhac uong thuoc
  - `prescription_new` - don thuoc moi duoc tao
  - `diagnostic_complete` - chan doan AI hoan tat
  - `promotion` - khuyen mai
  - `system` - thong bao he thong (bao tri, version moi, ...)
- **`referenceType` + `referenceId`**: lien ket polymorphic (vi du `referenceType="order", referenceId=123` -> tro toi don hang). FE click noti -> dieu huong theo cap nay.
- **MarkRead idempotent**: noti da doc se khong update `ReadAt` lan 2.
- **MarkAllRead bulk**: dung `ExecuteUpdateAsync` 1 round-trip toi DB, tra ve so noti vua chuyen tu unread -> read.
- **Index `(user_id, is_read)`** -> count chua doc + filter cuc nhanh.
- **Cascade delete**: xoa user -> tat ca noti xoa theo (DB-enforced).

---

### 13.1 `GET /api/notifications`

Liet ke thong bao cua user (newest first), co paging va filter.

**Query**: `?isRead=false&type=order_update&page=1&pageSize=20`

**Curl**

```bash
curl "http://localhost:5292/api/notifications?isRead=false" \
  -H "Authorization: Bearer $TOKEN"
```

**Response 200**: `PagedResult<NotificationDto>`.

```json
{
  "items": [
    {
      "id": 12,
      "notificationType": "order_update",
      "title": "Don hang ORD-20260430-XYZ da duoc xac nhan",
      "body": "Don hang cua ban dang duoc chuan bi de giao.",
      "referenceType": "order",
      "referenceId": 1,
      "isRead": false,
      "readAt": null,
      "createdAt": "2026-04-30T10:00:00"
    }
  ],
  "page": 1, "pageSize": 20, "totalCount": 1,
  "totalPages": 1, "hasNext": false, "hasPrevious": false
}
```

---

### 13.2 `GET /api/notifications/unread-count`

Dem so noti chua doc cua user. **Goi tu icon chuong** o header.

**Curl**

```bash
curl http://localhost:5292/api/notifications/unread-count \
  -H "Authorization: Bearer $TOKEN"
```

**Response 200**

```json
{ "count": 3 }
```

| Status | Khi nao              |
| ------ | -------------------- |
| 200    | OK                   |
| 401    | thieu token          |

---

### 13.3 `PUT /api/notifications/{id}/read`

Danh dau 1 noti da doc. **Idempotent**: goi nhieu lan voi noti da read se giu nguyen `readAt` lan dau.

**Curl**

```bash
curl -X PUT http://localhost:5292/api/notifications/12/read \
  -H "Authorization: Bearer $TOKEN"
```

**Response 200**: `NotificationDto` voi `isRead=true`, `readAt` da set.

| Status | Khi nao              |
| ------ | -------------------- |
| 200    | OK                   |
| 401    | thieu token          |
| 403    | noti cua user khac   |
| 404    | noti khong ton tai   |

---

### 13.4 `PUT /api/notifications/read-all`

Danh dau tat ca noti chua doc cua user thanh da doc. Dung 1 query bulk-update.

**Curl**

```bash
curl -X PUT http://localhost:5292/api/notifications/read-all \
  -H "Authorization: Bearer $TOKEN"
```

**Response 200**

```json
{ "updatedCount": 3 }
```

> Neu user khong co noti chua doc -> `updatedCount: 0` (van la 200).

| Status | Khi nao              |
| ------ | -------------------- |
| 200    | OK                   |
| 401    | thieu token          |

---

### 13.5 `DELETE /api/notifications/{id}`

Xoa 1 noti khoi danh sach cua user.

**Curl**

```bash
curl -X DELETE http://localhost:5292/api/notifications/12 \
  -H "Authorization: Bearer $TOKEN"
```

**Response 204** (No Content).

| Status | Khi nao              |
| ------ | -------------------- |
| 204    | OK                   |
| 401    | thieu token          |
| 403    | noti cua user khac   |
| 404    | noti khong ton tai   |

---

## 14. Diagnostic AI (Chan doan AI)

> **Phan lon endpoint yeu cau JWT** (tru `GET /api/symptoms` la public catalog).
> Module nay la **diem nhan** cua PharmaIntel AI: user chon trieu chung -> chat voi AI -> nhan ket luan + thuoc de xuat.

### Architecture

```
Symptom (catalog public)
   |
   v
DiagnosticSession (1:N) ----+--- DiagnosticSessionSymptom (N:N voi Symptom)
                            |
                            +--- DiagnosticMessage (1:N - chat history)
                            |
                            +--- DiagnosticResult (1:1) ---+--- DiagnosticResultMedication (N:N voi Medication)
```

### Engine

- Hien tai dung **`MockDiagnosticEngine`** (rule-based theo keyword tieng Viet khong dau).
- Logic mock:
  - Phat hien red-flag (`dau nguc`, `kho tho`, `ngat`, `co giat`, ...) -> `riskLevel="emergency"`, requiresDoctorVisit=true.
  - Cum trieu chung cam cum (`sot`, `ho`, `so mui`, `dau hong`) -> low-risk + recommend paracetamol/ibuprofen.
  - Dau dau / chong mat -> low-risk + recommend giam dau.
  - Dau bung / buon non / tieu chay -> medium-risk + recommend smecta/men tieu hoa.
  - Default -> low-risk + theo doi them.
- **Suggested medications**: query top 3 thuoc trong DB (`is_active=true`, `is_prescription_required=false`) match keyword.
- **Khi chuyen sang AI that** (OpenAI/Azure/Gemini): chi can tao `OpenAIDiagnosticEngine : IDiagnosticEngine` va register lai DI - khong sua service/controller.
- Audit: moi `DiagnosticResult` luu `modelName + modelVersion` (`PharmaIntel-MockEngine` / `0.1-rule-based`) de truy vet.

### Quy tac nghiep vu

- **Session moi tao luon `status="in_progress"`**, kem auto-message:
  - `system`: "Trieu chung da chon: ..."
  - `user`: noi dung `initialMessage` (neu co)
  - `ai`: phan hoi tu dong tu mock engine
- **AddMessage chi cho khi `in_progress`** (cancelled/completed/failed -> 409). Moi user message duoc AI auto-reply ngay (sau 3 user message AI goi y `/complete`).
- **Complete chi cho khi `in_progress`** -> chuyen `analyzing` -> chay engine -> tao result + suggested medications -> chuyen `completed`. Toan bo trong 1 transaction. Loi engine -> chuyen `failed`.
- **Result 1:1 voi session** (`UQ_diagnostic_results_session_id`). Re-complete -> 409.
- **Ownership**: moi op kiem tra `userId`. Truy cap session/result cua user khac -> 403.
- **`symptomIds` validation**: bat buoc >= 1 va toi da 20; tat ca phai ton tai trong DB.

### Field reference

| Field          | Type    | Notes                                                  |
| -------------- | ------- | ------------------------------------------------------ |
| Session `status`| string | `in_progress`, `analyzing`, `completed`, `cancelled`, `failed` |
| Message `senderType` | string | `user`, `ai`, `system`                            |
| Result `riskLevel` | string | `low`, `medium`, `high`, `emergency`                 |
| Result `confidenceScore` | decimal(5,2) | 0..100 (CHECK constraint o DB)             |

---

### 14.1 `GET /api/symptoms`

Danh muc trieu chung. **Public** - khong yeu cau JWT. Dung de FE render checklist khi user bat dau session.

**Query**: `?groupName=ho-hap` (optional - filter theo nhom).

**Curl**

```bash
curl http://localhost:5292/api/symptoms
```

**Response 200**

```json
[
  { "id": 1, "name": "Sot", "groupName": "toan than", "displayOrder": 1 },
  { "id": 2, "name": "Ho", "groupName": "ho hap", "displayOrder": 1 },
  { "id": 3, "name": "So mui", "groupName": "ho hap", "displayOrder": 2 }
]
```

> Sap xep: `groupName` -> `displayOrder` -> `name`.

---

### 14.2 `POST /api/diagnostics/sessions`

Bat dau phien chan doan moi (chon trieu chung + tin nhan dau tien).

**Request body**

```json
{
  "symptomIds": [1, 2, 3],
  "initialMessage": "Toi bi sot tu chieu hom qua, kem theo ho khan"
}
```

- `symptomIds`: bat buoc >= 1, toi da 20 (validator).
- `initialMessage`: optional, max 2000 ky tu.

**Curl**

```bash
curl -X POST http://localhost:5292/api/diagnostics/sessions \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"symptomIds":[1,2,3],"initialMessage":"Toi bi sot tu chieu hom qua"}'
```

**Response 201**: `DiagnosticSessionDto` (status `in_progress`, da co system + user + ai messages).

```json
{
  "id": 1,
  "status": "in_progress",
  "messageCount": 3,
  "symptoms": ["Sot", "Ho", "So mui"],
  "createdAt": "...", "completedAt": null,
  "messages": [
    { "id": 1, "sessionId": 1, "senderType": "system", "content": "Trieu chung da chon: Sot, Ho, So mui", "sentAt": "..." },
    { "id": 2, "sessionId": 1, "senderType": "user", "content": "Toi bi sot tu chieu hom qua", "sentAt": "..." },
    { "id": 3, "sessionId": 1, "senderType": "ai", "content": "Da ghi nhan. Vui long mo ta them...", "sentAt": "..." }
  ],
  "result": null
}
```

| Status | Khi nao                                                  |
| ------ | -------------------------------------------------------- |
| 201    | OK                                                       |
| 400    | symptomIds rong / > 20 / chua gia tri khong hop le        |
| 401    | thieu token                                              |
| 404    | symptomId khong ton tai trong DB                          |

---

### 14.3 `GET /api/diagnostics/sessions/my`

List phien chan doan cua user, paged + filter theo status.

**Query**: `?status=completed&page=1&pageSize=20`

```bash
curl "http://localhost:5292/api/diagnostics/sessions/my?status=completed" \
  -H "Authorization: Bearer $TOKEN"
```

**Response 200**: `PagedResult<DiagnosticSessionListItemDto>` (newest first).

```json
{
  "items": [
    {
      "id": 1, "status": "completed", "messageCount": 5,
      "symptoms": ["Sot", "Ho", "So mui"],
      "createdAt": "...", "completedAt": "..."
    }
  ],
  "page": 1, "pageSize": 20, "totalCount": 1,
  "totalPages": 1, "hasNext": false, "hasPrevious": false
}
```

---

### 14.4 `GET /api/diagnostics/sessions/{id}`

Chi tiet phien chan doan kem `messages` + `result` (neu da complete). **Chi owner xem duoc**.

```bash
curl http://localhost:5292/api/diagnostics/sessions/1 -H "Authorization: Bearer $TOKEN"
```

**Response 200**: `DiagnosticSessionDto`.

| Status | Khi nao              |
| ------ | -------------------- |
| 200    | OK                   |
| 401    | thieu token          |
| 403    | session cua user khac|
| 404    | session khong ton tai|

---

### 14.5 `POST /api/diagnostics/sessions/{id}/messages`

Gui tin nhan moi vao phien. AI tu dong tra loi (mock engine).

**Request body**

```json
{ "content": "Toi cung bi nhuc moi nguoi va dau dau" }
```

**Curl**

```bash
curl -X POST http://localhost:5292/api/diagnostics/sessions/1/messages \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"content":"Toi cung bi nhuc moi nguoi va dau dau"}'
```

**Response 201**: `DiagnosticMessageDto` cua **user message**. AI reply duoc luu cung; lay full chat qua 14.4.

| Status | Khi nao                                                |
| ------ | ------------------------------------------------------ |
| 201    | OK                                                     |
| 400    | content rong / > 2000 ky tu                            |
| 401    | thieu token                                            |
| 403    | session cua user khac                                  |
| 404    | session khong ton tai                                  |
| 409    | session khong o trang thai `in_progress`               |

---

### 14.6 `POST /api/diagnostics/sessions/{id}/complete`

Ket thuc phien va chay engine. Flow:
1. Status `in_progress` -> `analyzing`.
2. Engine sinh ket luan + suggest medications.
3. Tao `DiagnosticResult` + `DiagnosticResultMedication[]` + AI summary message.
4. Status -> `completed`, `completedAt` set.
5. Toan bo trong 1 transaction. Loi engine -> status `failed`.

**Curl**

```bash
curl -X POST http://localhost:5292/api/diagnostics/sessions/1/complete \
  -H "Authorization: Bearer $TOKEN"
```

**Response 200**: `DiagnosticSessionDto` voi `status="completed"` va `result != null` chua thuoc de xuat.

```json
{
  "id": 1,
  "status": "completed",
  "completedAt": "...",
  "result": {
    "id": 1, "sessionId": 1,
    "aiConclusion": "Co kha nang ban dang bi cam cum thong thuong...",
    "confidenceScore": 75.0,
    "riskLevel": "low",
    "redFlags": null,
    "requiresDoctorVisit": false,
    "modelName": "PharmaIntel-MockEngine",
    "modelVersion": "0.1-rule-based",
    "diagnosedAt": "...",
    "suggestedMedications": [
      {
        "id": 1, "medicationId": 5, "medicationName": "Paracetamol 500mg",
        "price": 15000, "discountPercent": 5,
        "imageUrl": "...", "isPrescriptionRequired": false, "priority": 1
      }
    ]
  }
}
```

| Status | Khi nao                                                |
| ------ | ------------------------------------------------------ |
| 200    | OK                                                     |
| 401    | thieu token                                            |
| 403    | session cua user khac                                  |
| 404    | session khong ton tai                                  |
| 409    | session khong o trang thai `in_progress`               |

---

### 14.7 `GET /api/diagnostics/results/{id}`

Chi tiet 1 ket qua (lookup truc tiep theo result.id, vi du tu link share noti).

```bash
curl http://localhost:5292/api/diagnostics/results/1 -H "Authorization: Bearer $TOKEN"
```

**Response 200**: `DiagnosticResultDto` voi `suggestedMedications` sap xep theo priority.

| Status | Khi nao              |
| ------ | -------------------- |
| 200    | OK                   |
| 401    | thieu token          |
| 403    | result cua user khac |
| 404    | result khong ton tai |

---

## 15. Test Script Tong Hop

```bash
#!/usr/bin/env bash
BASE=http://localhost:5292

echo "[1] HEALTH"
curl -s $BASE/api/health
echo

echo "[2] DB-CHECK"
curl -s $BASE/api/db-check | head -c 200
echo

echo "[3] REGISTER"
curl -s -X POST $BASE/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"fullName":"Tester","email":"tester@example.com","password":"Password123","isTermsAccepted":true}'
echo

echo "[4] LOGIN"
TOKEN=$(curl -s -X POST $BASE/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"tester@example.com","password":"Password123"}' \
  | grep -oE '"accessToken":"[^"]+"' | sed 's/"accessToken":"//;s/"$//')
echo "Token: ${TOKEN:0:40}..."

echo "[5] ME"
curl -s $BASE/api/auth/me -H "Authorization: Bearer $TOKEN"
echo
```

---

## 16. Ghi chu chung

- **CORS**: cho phep `http://localhost:5173` (Vite) va `http://localhost:3000`. Sua trong `appsettings.json` -> `Cors:AllowedOrigins`.
- **Swagger**: chi bat o moi truong Development (`ASPNETCORE_ENVIRONMENT=Development`).
- **Loi 500**: xem log chi tiet trong terminal dotnet run hoac /tmp/api.log neu chay background.
- **Kill API dang chay (Windows)**:
  ```bash
  taskkill //F //IM PharmaIntel.API.exe
  ```
- **Reset user test** (khi muon dang ky lai cung email):
  ```sql
  -- mo SSMS hoac Azure Data Studio noi den (localdb)\MSSQLLocalDB
  USE PharmaIntelDB;
  DELETE FROM users WHERE email = 'test@example.com';
  ```

---

## 17. Endpoint Roadmap (chua lam)

| Module                      | Status   |
| --------------------------- | -------- |
| Validation + Error handling | DONE     |
| Auth                        | DONE     |
| Categories CRUD | DONE     |
| Medications CRUD| DONE     |
| Cart            | DONE     |
| Order / Checkout| DONE     |
| Address CRUD    | DONE     |
| User profile    | DONE     |
| Prescription    | DONE     |
| Medication Reminder | DONE |
| Health Metrics  | DONE     |
| Notification    | DONE     |
| Diagnostic AI   | DONE (mock engine) |
| PaymentMethod   | TODO     |
| Diagnostic AI   | TODO     |
| Notification    | TODO     |
| ...             | ...      |

> Khi them endpoint moi, **bo sung muc tuong ung vao file nay** de giu lai context test.
