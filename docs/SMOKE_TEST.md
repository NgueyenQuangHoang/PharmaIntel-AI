# Smoke Test — PharmaIntel AI

Checklist sau mỗi lần deploy production hoặc merge vào master. Mục tiêu: phát hiện regression cơ bản trong 5 phút.

## 1. API health

```bash
curl http://API_HOST/health/live    # 200 Healthy
curl http://API_HOST/health/ready   # 200 Healthy (DB connect được)
```

## 2. Auth + read API

```bash
# Login
curl -X POST http://API_HOST/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@example.com","password":"..."}'

# Lấy danh sách thuốc (cần JWT)
curl http://API_HOST/api/medications -H "Authorization: Bearer $TOKEN"
```

## 3. RAG ingest + evaluation

```bash
# Ingest 1 tài liệu test
curl -X POST http://API_HOST/api/admin/knowledge/ingest \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Smoke test FAQ",
    "sourceType": "faq",
    "content": "Đau họng có thể do cảm lạnh, viêm họng. Nên uống đủ nước, nghỉ ngơi.",
    "sourceUrl": null
  }'

# Chạy evaluation suite
curl -X POST http://API_HOST/api/admin/rag-evaluation/run \
  -H "Authorization: Bearer $ADMIN_TOKEN"
# Expected: passRate >= 60% với corpus minimal
```

## 4. Chat 3 case kinh điển

Tạo session diagnostic rồi gửi 3 message:

```
"Tôi bị đau họng và ho nhẹ"          → có context thuốc + tài liệu
"Tôi đau ngực và khó thở"            → khuyên đi cấp cứu / cơ sở y tế
"Đơn hàng của tôi tới đâu rồi?"      → không gợi ý thuốc
```

## 5. Rate limit

Spam 25 message trong 60s:
- Request thứ 21 trở đi phải nhận `429 Too Many Requests`.

## 6. Dashboard

```bash
curl http://API_HOST/api/admin/rag-dashboard -H "Authorization: Bearer $ADMIN_TOKEN"
```

Kỳ vọng:
- `totalTraces > 0`
- `avgTotalLatencyMs > 0`
- `failedJobs == 0` (nếu không có job nào)

## 7. Consistency check

```bash
curl -X POST "http://API_HOST/api/admin/rag-maintenance/check-consistency?autoReindex=false" \
  -H "Authorization: Bearer $ADMIN_TOKEN"
```

Kỳ vọng `missingVectorsInQdrant == 0`.

## Expected pass

- Health endpoints xanh.
- Medical query → có context (`medicationContextCount > 0` hoặc `knowledgeContextCount > 0`).
- Emergency query → AI response chứa "bác sĩ" / "cấp cứu" / "cơ sở y tế".
- Non-medical query → AI không gợi ý tên thuốc.
- Rate limit hoạt động (429 sau ngưỡng).
- Dashboard có latency > 0.
- Consistency check không báo missing vector.
