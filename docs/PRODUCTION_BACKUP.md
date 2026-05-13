# Production Backup — PharmaIntel AI

Phase 5 hardening: backup chiến lược cho SQL Server + Qdrant để có thể restore dữ liệu RAG.

## SQL Server

Backup hằng ngày (full) + 4 giờ/lần (differential):

Các bảng tối thiểu phải có trong backup:
- `users`, `refresh_tokens`
- `medications`, `categories`, `prescriptions`, `prescription_items`
- `diagnostic_sessions`, `diagnostic_messages`, `diagnostic_results`, `diagnostic_result_medications`
- `knowledge_documents`, `knowledge_chunks`
- `rag_traces`, `ai_response_feedbacks`
- `embedding_cache`, `rag_jobs`
- `orders`, `order_items`

Command mẫu:
```sql
BACKUP DATABASE [PharmaIntelDB] TO DISK = 'D:\backup\pharmaintel_full_{date}.bak'
  WITH FORMAT, COMPRESSION, CHECKSUM;
```

## Qdrant

Backup volume `qdrant-data` (docker volume). Hai cách:

1. **Volume snapshot** (cron):
   ```bash
   docker run --rm -v pharmaintel-qdrant-data:/data -v $(pwd):/backup alpine \
     tar czf /backup/qdrant_$(date +%F).tar.gz -C /data .
   ```

2. **Qdrant snapshot API**:
   ```bash
   curl -X POST http://localhost:6333/collections/pharmaintel_knowledge/snapshots
   curl -O http://localhost:6333/collections/pharmaintel_knowledge/snapshots/{snapshot_name}
   ```

## Restore order

1. Restore SQL Server từ `.bak` mới nhất (full + differential nếu có).
2. Restore Qdrant volume hoặc upload snapshot trở lại collection.
3. Start API. Migration tự apply (`Bootstrap.MigrateOnStartup=true`).
4. Chạy consistency check: `POST /api/admin/rag-maintenance/check-consistency?autoReindex=true`.
5. Nếu missing vectors → `RagJobWorker` tự reindex các document được flag.

## Retention

- SQL full backup: giữ 30 ngày.
- SQL differential: giữ 7 ngày.
- Qdrant snapshot: giữ 14 ngày.
- `rag_traces` + `embedding_cache`: cân nhắc retention 90 ngày để bảng không phình (chưa có job dọn — phase sau).

## Secrets checklist (production deploy)

- [ ] `JWT_KEY` >= 32 ký tự, set qua env var.
- [ ] `GEMINI_API_KEY` không nằm trong git, set qua env var hoặc secrets manager.
- [ ] `SA_PASSWORD` SQL Server không nằm trong git.
- [ ] `Qdrant__BaseUrl` trỏ vào hostname nội bộ (không expose 6333 ra internet).
- [ ] CORS chỉ allow domain frontend production.
- [ ] Swagger tắt ở Production (`if (env.IsDevelopment())` đã đúng).
