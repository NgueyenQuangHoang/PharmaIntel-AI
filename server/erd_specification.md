# Đặc tả ERD PharmaIntel AI — SQL Server

> Phiên bản: v3  
> CSDL mục tiêu: Microsoft SQL Server  
> Mục tiêu: đặc tả bảng, quan hệ, ràng buộc và quy tắc nghiệp vụ để backend/database team triển khai.

---

## 1. Quy ước chung

### 1.1. Kiểu dữ liệu

| Dữ liệu | Kiểu SQL Server |
|---|---|
| Khóa chính | `BIGINT IDENTITY(1,1)` |
| Text tiếng Việt | `NVARCHAR(...)` |
| Text dài | `NVARCHAR(MAX)` |
| Boolean | `BIT` |
| Ngày giờ | `DATETIME2(0)` |
| Ngày | `DATE` |
| Giờ | `TIME(0)` |
| Tiền | `DECIMAL(12,2)` |
| Phần trăm | `DECIMAL(5,2)` |
| JSON đơn giản | `NVARCHAR(MAX)` kèm `ISJSON(...)` nếu cần kiểm tra |

### 1.2. Quy tắc thiết kế

- Tất cả bảng chính dùng khóa chính `id`.
- Tất cả dữ liệu tiếng Việt dùng `NVARCHAR`.
- Không dùng `ENUM`; dùng `NVARCHAR + CHECK`.
- Không dùng `TIMESTAMP` cho ngày giờ trong SQL Server.
- Dữ liệu đơn hàng và thanh toán cần giữ snapshot tại thời điểm phát sinh.
- Dữ liệu sức khỏe/đơn thuốc nên hạn chế xóa cứng.
- Backend chịu trách nhiệm cập nhật `updated_at` khi update record.

---

## 2. Danh sách bảng

| STT | Bảng | Nhóm | Mục đích |
|---:|---|---|---|
| 1 | `users` | Người dùng | Tài khoản khách hàng |
| 2 | `user_settings` | Người dùng | Cài đặt cá nhân |
| 3 | `user_consents` | Tuân thủ | Lịch sử đồng ý điều khoản/quyền riêng tư |
| 4 | `doctors` | Y tế | Danh mục bác sĩ |
| 5 | `pharmacists` | Y tế | Danh mục dược sĩ |
| 6 | `notifications` | Hệ thống | Thông báo người dùng |
| 7 | `addresses` | Giao hàng | Địa chỉ giao hàng |
| 8 | `payment_methods` | Thanh toán | Phương thức thanh toán của user |
| 9 | `categories` | Sản phẩm | Danh mục thuốc |
| 10 | `medications` | Sản phẩm | Thuốc/dược phẩm |
| 11 | `symptoms` | AI Diagnostic | Danh mục triệu chứng |
| 12 | `diagnostic_sessions` | AI Diagnostic | Phiên chẩn đoán |
| 13 | `diagnostic_session_symptoms` | AI Diagnostic | Triệu chứng được chọn trong phiên |
| 14 | `diagnostic_messages` | AI Diagnostic | Chat trong phiên |
| 15 | `diagnostic_results` | AI Diagnostic | Kết quả chẩn đoán |
| 16 | `diagnostic_result_medications` | AI Diagnostic | Thuốc AI đề xuất |
| 17 | `prescriptions` | Đơn thuốc | Hồ sơ đơn thuốc |
| 18 | `prescription_items` | Đơn thuốc | Thuốc trong đơn |
| 19 | `prescription_documents` | Đơn thuốc | File ảnh/PDF đơn thuốc |
| 20 | `medication_reminders` | Nhắc thuốc | Cấu hình lịch nhắc |
| 21 | `medication_reminder_logs` | Nhắc thuốc | Log từng lần nhắc |
| 22 | `health_metrics` | Sức khỏe | Chỉ số sức khỏe |
| 23 | `cart_items` | Giỏ hàng | Sản phẩm trong giỏ |
| 24 | `orders` | Đơn hàng | Đơn hàng |
| 25 | `order_items` | Đơn hàng | Chi tiết đơn hàng |
| 26 | `payment_transactions` | Thanh toán | Giao dịch thanh toán |
| 27 | `pharmacist_chat_sessions` | Chat | Phiên chat với dược sĩ |
| 28 | `pharmacist_chat_messages` | Chat | Tin nhắn chat với dược sĩ |
| 29 | `ai_insights` | AI | Insight cá nhân hóa |
| 30 | `audit_logs` | Audit | Nhật ký thao tác nhạy cảm |

---

## 3. Đặc tả bảng

### 3.1. `users`

Lưu tài khoản người dùng cuối.

| Cột | Kiểu | Null | Ghi chú |
|---|---|---:|---|
| `id` | `BIGINT IDENTITY` | No | PK |
| `full_name` | `NVARCHAR(255)` | No | Họ tên |
| `email` | `NVARCHAR(255)` | No | Unique |
| `password_hash` | `NVARCHAR(255)` | Yes | Null nếu chỉ OAuth |
| `avatar_url` | `NVARCHAR(500)` | Yes | Ảnh đại diện |
| `auth_provider` | `NVARCHAR(20)` | No | `local`, `google`, `apple` |
| `auth_provider_id` | `NVARCHAR(255)` | Yes | ID từ OAuth |
| `is_terms_accepted` | `BIT` | No | Đã chấp nhận điều khoản |
| `is_active` | `BIT` | No | Soft deactivate |
| `created_at` | `DATETIME2(0)` | No | UTC |
| `updated_at` | `DATETIME2(0)` | No | UTC |

Ràng buộc:
- `UQ_users_email`
- `UX_users_auth_provider_id` filtered unique khi `auth_provider_id IS NOT NULL`
- `CK_users_auth_provider`

---

### 3.2. `user_settings`

Cài đặt 1:1 với user.

| Cột | Kiểu | Null | Ghi chú |
|---|---|---:|---|
| `id` | `BIGINT IDENTITY` | No | PK |
| `user_id` | `BIGINT` | No | FK → `users.id`, unique |
| `dark_mode` | `NVARCHAR(20)` | No | `light`, `dark`, `system` |
| `language_code` | `NVARCHAR(10)` | No | Mặc định `vi` |
| `notification_enabled` | `BIT` | No | Bật/tắt thông báo |
| `reminder_sound_enabled` | `BIT` | No | Bật/tắt âm thanh nhắc thuốc |
| `created_at` | `DATETIME2(0)` | No | UTC |
| `updated_at` | `DATETIME2(0)` | No | UTC |

---

### 3.3. `user_consents`

Lưu lịch sử đồng ý điều khoản, quyền riêng tư, cảnh báo AI.

| Cột | Kiểu | Null | Ghi chú |
|---|---|---:|---|
| `id` | `BIGINT IDENTITY` | No | PK |
| `user_id` | `BIGINT` | No | FK → `users.id` |
| `consent_type` | `NVARCHAR(50)` | No | `terms`, `privacy`, `medical_ai_disclaimer`, `marketing` |
| `consent_version` | `NVARCHAR(50)` | No | Version nội dung |
| `accepted_at` | `DATETIME2(0)` | No | Thời điểm đồng ý |
| `revoked_at` | `DATETIME2(0)` | Yes | Thời điểm thu hồi |
| `ip_address` | `NVARCHAR(45)` | Yes | IPv4/IPv6 |
| `user_agent` | `NVARCHAR(500)` | Yes | Thiết bị/trình duyệt |

---

### 3.4. `doctors`

Danh mục bác sĩ kê đơn. `doctor_id` trong đơn thuốc có thể null nếu bác sĩ ngoài hệ thống.

| Cột | Kiểu | Null | Ghi chú |
|---|---|---:|---|
| `id` | `BIGINT IDENTITY` | No | PK |
| `full_name` | `NVARCHAR(255)` | No | Tên bác sĩ |
| `license_number` | `NVARCHAR(100)` | Yes | Số chứng chỉ hành nghề |
| `specialization` | `NVARCHAR(255)` | Yes | Chuyên khoa |
| `hospital` | `NVARCHAR(255)` | Yes | Bệnh viện/phòng khám |
| `phone` | `NVARCHAR(20)` | Yes | SĐT |
| `email` | `NVARCHAR(255)` | Yes | Email |
| `avatar_url` | `NVARCHAR(500)` | Yes | Ảnh |
| `is_active` | `BIT` | No | Đang hoạt động |
| `created_at` | `DATETIME2(0)` | No | UTC |
| `updated_at` | `DATETIME2(0)` | No | UTC |

---

### 3.5. `pharmacists`

Danh mục dược sĩ hỗ trợ chat và xác minh đơn thuốc.

| Cột | Kiểu | Null | Ghi chú |
|---|---|---:|---|
| `id` | `BIGINT IDENTITY` | No | PK |
| `full_name` | `NVARCHAR(255)` | No | Tên dược sĩ |
| `license_number` | `NVARCHAR(100)` | No | Unique |
| `specialization` | `NVARCHAR(255)` | Yes | Chuyên môn |
| `phone` | `NVARCHAR(20)` | Yes | SĐT |
| `email` | `NVARCHAR(255)` | Yes | Email |
| `avatar_url` | `NVARCHAR(500)` | Yes | Ảnh |
| `is_online` | `BIT` | No | Online/offline |
| `is_active` | `BIT` | No | Đang hoạt động |
| `created_at` | `DATETIME2(0)` | No | UTC |
| `updated_at` | `DATETIME2(0)` | No | UTC |

---

### 3.6. `notifications`

Thông báo hiển thị qua icon chuông/header.

| Cột | Kiểu | Null | Ghi chú |
|---|---|---:|---|
| `id` | `BIGINT IDENTITY` | No | PK |
| `user_id` | `BIGINT` | No | FK → `users.id` |
| `notification_type` | `NVARCHAR(50)` | No | Loại thông báo |
| `title` | `NVARCHAR(255)` | No | Tiêu đề |
| `body` | `NVARCHAR(MAX)` | Yes | Nội dung |
| `reference_type` | `NVARCHAR(50)` | Yes | Ví dụ: `order`, `prescription` |
| `reference_id` | `BIGINT` | Yes | ID đối tượng liên quan |
| `is_read` | `BIT` | No | Đã đọc |
| `read_at` | `DATETIME2(0)` | Yes | Thời điểm đọc |
| `created_at` | `DATETIME2(0)` | No | UTC |

---

### 3.7. `addresses`

Địa chỉ giao hàng của user.

| Cột | Kiểu | Null | Ghi chú |
|---|---|---:|---|
| `id` | `BIGINT IDENTITY` | No | PK |
| `user_id` | `BIGINT` | No | FK → `users.id` |
| `recipient_name` | `NVARCHAR(255)` | No | Người nhận |
| `phone` | `NVARCHAR(20)` | No | SĐT |
| `province` | `NVARCHAR(100)` | No | Tỉnh/thành |
| `district` | `NVARCHAR(100)` | No | Quận/huyện |
| `ward` | `NVARCHAR(100)` | No | Phường/xã |
| `street_address` | `NVARCHAR(500)` | No | Số nhà, đường |
| `is_default` | `BIT` | No | Địa chỉ mặc định |
| `is_active` | `BIT` | No | Còn sử dụng |
| `created_at` | `DATETIME2(0)` | No | UTC |
| `updated_at` | `DATETIME2(0)` | No | UTC |

Ràng buộc:
- `UX_addresses_user_default`: mỗi user chỉ có một địa chỉ mặc định.

---

### 3.8. `payment_methods`

Phương thức thanh toán user lưu/chọn.

| Cột | Kiểu | Null | Ghi chú |
|---|---|---:|---|
| `id` | `BIGINT IDENTITY` | No | PK |
| `user_id` | `BIGINT` | No | FK → `users.id` |
| `payment_type` | `NVARCHAR(30)` | No | `cod`, `bank_transfer`, `momo`, `zalopay`, `vnpay`, `credit_card` |
| `display_name` | `NVARCHAR(255)` | No | Tên hiển thị |
| `masked_account` | `NVARCHAR(255)` | Yes | Chỉ lưu masked |
| `provider_customer_id` | `NVARCHAR(255)` | Yes | Token/customer id từ provider |
| `is_default` | `BIT` | No | Mặc định |
| `is_active` | `BIT` | No | Còn dùng |
| `created_at` | `DATETIME2(0)` | No | UTC |
| `updated_at` | `DATETIME2(0)` | No | UTC |

Không lưu số thẻ hoặc thông tin ví gốc.

---

### 3.9. `categories`

Danh mục thuốc, hỗ trợ danh mục cha/con.

| Cột | Kiểu | Null | Ghi chú |
|---|---|---:|---|
| `id` | `BIGINT IDENTITY` | No | PK |
| `parent_id` | `BIGINT` | Yes | FK tự tham chiếu |
| `name` | `NVARCHAR(100)` | No | Tên danh mục |
| `slug` | `NVARCHAR(100)` | No | Unique |
| `icon` | `NVARCHAR(100)` | Yes | Icon UI |
| `display_order` | `INT` | No | Thứ tự hiển thị |
| `is_active` | `BIT` | No | Đang hiển thị |
| `created_at` | `DATETIME2(0)` | No | UTC |
| `updated_at` | `DATETIME2(0)` | No | UTC |

---

### 3.10. `medications`

Danh mục thuốc/dược phẩm.

| Cột | Kiểu | Null | Ghi chú |
|---|---|---:|---|
| `id` | `BIGINT IDENTITY` | No | PK |
| `sku` | `NVARCHAR(100)` | No | Unique |
| `name` | `NVARCHAR(255)` | No | Tên thuốc |
| `generic_name` | `NVARCHAR(255)` | Yes | Hoạt chất/tên generic |
| `manufacturer` | `NVARCHAR(255)` | Yes | Nhà sản xuất |
| `registration_number` | `NVARCHAR(100)` | Yes | Số đăng ký nếu có |
| `description` | `NVARCHAR(MAX)` | Yes | Mô tả |
| `dosage` | `NVARCHAR(100)` | Yes | Hàm lượng |
| `packaging` | `NVARCHAR(100)` | Yes | Quy cách |
| `price` | `DECIMAL(12,2)` | No | Giá |
| `discount_percent` | `DECIMAL(5,2)` | No | 0–100 |
| `category_id` | `BIGINT` | No | FK → `categories.id` |
| `usage_instructions` | `NVARCHAR(MAX)` | Yes | Hướng dẫn dùng |
| `benefits` | `NVARCHAR(MAX)` | Yes | Lợi ích |
| `active_ingredients` | `NVARCHAR(MAX)` | Yes | Hoạt chất |
| `contraindications` | `NVARCHAR(MAX)` | Yes | Chống chỉ định |
| `side_effects` | `NVARCHAR(MAX)` | Yes | Tác dụng phụ |
| `storage_instructions` | `NVARCHAR(MAX)` | Yes | Bảo quản |
| `image_url` | `NVARCHAR(500)` | Yes | Ảnh |
| `is_featured` | `BIT` | No | Nổi bật |
| `is_best_seller` | `BIT` | No | Bán chạy |
| `is_prescription_required` | `BIT` | No | Cần đơn thuốc |
| `stock_quantity` | `INT` | No | Tồn kho đơn giản |
| `is_active` | `BIT` | No | Đang bán |
| `created_at` | `DATETIME2(0)` | No | UTC |
| `updated_at` | `DATETIME2(0)` | No | UTC |

---

### 3.11. Nhóm AI Diagnostic

#### `symptoms`
Danh mục triệu chứng.

#### `diagnostic_sessions`
Phiên chẩn đoán AI của user.

Trạng thái:
- `in_progress`
- `analyzing`
- `completed`
- `cancelled`
- `failed`

#### `diagnostic_session_symptoms`
Bảng N:N giữa phiên chẩn đoán và triệu chứng.

Ràng buộc:
- Unique `session_id + symptom_id`

#### `diagnostic_messages`
Tin nhắn giữa user và AI trong phiên.

Sender:
- `user`
- `ai`
- `system`

#### `diagnostic_results`
Kết quả AI. Mỗi session tối đa một result.

Các cột quan trọng:
- `confidence_score`: 0–100
- `risk_level`: `low`, `medium`, `high`, `emergency`
- `red_flags`
- `requires_doctor_visit`
- `model_name`
- `model_version`

#### `diagnostic_result_medications`
Thuốc AI đề xuất cho result.

Ràng buộc:
- Unique `result_id + medication_id`
- `priority > 0`

---

### 3.12. Nhóm đơn thuốc

#### `prescriptions`

Đơn thuốc của user.

Trạng thái đơn:
- `draft`
- `active`
- `completed`
- `expired`
- `cancelled`

Trạng thái xác minh:
- `not_required`
- `pending`
- `verified`
- `rejected`

Lưu ý:
- `doctor_id` nullable.
- `doctor_name_snapshot` lưu tên bác sĩ tại thời điểm tạo đơn.

#### `prescription_items`

Danh sách thuốc trong đơn.

Lưu `medication_name` dạng snapshot để giữ lịch sử nếu thuốc trong master data bị đổi tên.

#### `prescription_documents`

File đơn thuốc upload.

Trạng thái:
- `pending`
- `verified`
- `rejected`

Có thể gắn dược sĩ xác minh qua `verified_by_pharmacist_id`.

---

### 3.13. Nhóm nhắc thuốc và sức khỏe

#### `medication_reminders`

Cấu hình lịch nhắc.

Tần suất:
- `once`
- `daily`
- `weekly`
- `custom`

Trạng thái:
- `active`
- `paused`
- `completed`
- `cancelled`

#### `medication_reminder_logs`

Log từng lần nhắc.

Trạng thái:
- `scheduled`
- `taken`
- `missed`
- `skipped`

#### `health_metrics`

Chỉ số sức khỏe.

Loại chỉ số:
- `blood_pressure`
- `heart_rate`
- `temperature`
- `weight`
- `blood_sugar`
- `oxygen_saturation`

Với huyết áp, dùng:
- `value_number`: tâm thu
- `value_number_2`: tâm trương
- `unit`: `mmHg`

---

### 3.14. Nhóm giỏ hàng và đơn hàng

#### `cart_items`

Giỏ hàng.

Ràng buộc:
- Unique `user_id + medication_id`
- `quantity > 0`

#### `orders`

Đơn hàng.

Trạng thái đơn:
- `pending`
- `confirmed`
- `processing`
- `shipping`
- `delivered`
- `cancelled`
- `refunded`

Trạng thái thanh toán:
- `unpaid`
- `pending`
- `paid`
- `failed`
- `refunded`
- `cod_pending`

Các snapshot quan trọng:
- `shipping_recipient_name`
- `shipping_phone`
- `shipping_full_address`
- `payment_type_snapshot`

#### `order_items`

Chi tiết đơn hàng.

Lưu snapshot:
- `medication_name_snapshot`
- `unit_price`
- `discount_percent`
- `total_price`

#### `payment_transactions`

Giao dịch thanh toán.

Trạng thái:
- `initiated`
- `pending`
- `success`
- `failed`
- `cancelled`
- `refunded`

---

### 3.15. Nhóm chat dược sĩ

#### `pharmacist_chat_sessions`

Phiên chat giữa user và dược sĩ.

Trạng thái:
- `open`
- `waiting`
- `closed`
- `cancelled`

#### `pharmacist_chat_messages`

Tin nhắn trong phiên.

Sender:
- `user`
- `pharmacist`
- `system`

---

### 3.16. `ai_insights`

Insight AI cá nhân hóa.

Loại insight:
- `health_summary`
- `medication`
- `diagnostic`
- `lifestyle`
- `system`

---

### 3.17. `audit_logs`

Nhật ký thao tác nhạy cảm.

Nên ghi log cho:
- xác minh đơn thuốc;
- thay đổi trạng thái đơn hàng/thanh toán;
- xem/sửa dữ liệu sức khỏe;
- thay đổi thông tin y tế quan trọng.

---

## 4. Index quan trọng

| Index | Mục đích |
|---|---|
| `UQ_users_email` | Login bằng email |
| `UX_users_auth_provider_id` | OAuth login |
| `UX_addresses_user_default` | Một địa chỉ mặc định/user |
| `UX_payment_methods_user_default` | Một payment mặc định/user |
| `UQ_medications_sku` | Quản lý thuốc |
| `UQ_cart_items_user_medication` | Tránh trùng sản phẩm trong giỏ |
| `IX_orders_user_created_at` | Lịch sử đơn hàng user |
| `IX_notifications_user_read` | Danh sách thông báo chưa đọc |
| `IX_health_metrics_user_type_recorded_at` | Biểu đồ sức khỏe |
| `IX_medication_reminder_logs_reminder_scheduled_at` | Lịch sử nhắc thuốc |
| `IX_diagnostic_results_user_diagnosed_at` | Lịch sử chẩn đoán |
| `IX_payment_transactions_order_id` | Đối soát thanh toán |

---

## 5. Quy tắc nghiệp vụ nên enforce ở tầng service

1. Không cho checkout thuốc kê đơn nếu chưa có đơn thuốc/xác minh phù hợp.
2. Khi tạo đơn hàng, copy snapshot địa chỉ và phương thức thanh toán vào `orders`.
3. Khi giá thuốc thay đổi, không sửa giá trong `order_items` cũ.
4. Khi user cập nhật địa chỉ, không ảnh hưởng đơn hàng đã đặt.
5. Khi reminder chạy, tạo log ở `medication_reminder_logs`.
6. Khi AI tạo kết quả nguy cơ cao, tạo notification và khuyến nghị gặp bác sĩ.
7. Không lưu thông tin thẻ/ví gốc.
8. Các API đọc dữ liệu sức khỏe phải kiểm tra quyền truy cập.
9. Dữ liệu audit không nên cho phép sửa/xóa qua API thông thường.
10. Các field `updated_at` phải được cập nhật mỗi khi update.

---

## 6. Mapping module frontend → database

| Frontend | Bảng chính |
|---|---|
| `/login`, `/register` | `users`, `user_settings`, `user_consents` |
| Home/products | `categories`, `medications`, `ai_insights` |
| Diagnostic chat | `diagnostic_sessions`, `diagnostic_messages`, `symptoms` |
| Diagnostic result | `diagnostic_results`, `diagnostic_result_medications`, `notifications` |
| Medicine cabinet/shop | `medications`, `cart_items`, `orders`, `order_items` |
| Checkout | `addresses`, `payment_methods`, `orders`, `payment_transactions` |
| Profile | `users`, `prescriptions`, `health_metrics`, `medication_reminders` |
| Prescription upload | `prescriptions`, `prescription_items`, `prescription_documents` |
| Reminder | `medication_reminders`, `medication_reminder_logs`, `notifications` |
| Chat pharmacist | `pharmacists`, `pharmacist_chat_sessions`, `pharmacist_chat_messages` |

---

## 7. Ghi chú triển khai ERD bằng SQL Server

Sau khi chạy file `pharmaintel_erd_sqlserver.sql`, có thể vẽ ERD bằng:

- SQL Server Management Studio Database Diagrams
- Azure Data Studio extension
- DBeaver
- DataGrip
- dbForge Studio
- Visual Paradigm / draw.io import từ database nếu có connector

Nếu dùng công cụ không hỗ trợ `CHECK` hoặc filtered index khi import, các quan hệ FK vẫn đủ để dựng ERD.