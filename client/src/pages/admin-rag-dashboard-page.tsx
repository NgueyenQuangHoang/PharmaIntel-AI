import { useEffect, useState } from 'react'
import { AdminPageShell } from '@/features/admin/components/AdminPageShell'
import { StatCard } from '@/features/admin/components/StatCard'
import { ragApi, type RagDashboard } from '@/features/rag/rag-api'

export function AdminRagDashboardPage() {
  const [data, setData] = useState<RagDashboard | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const load = async () => {
    setLoading(true)
    setError(null)
    try {
      setData(await ragApi.getDashboard())
    } catch {
      setError('Không tải được dashboard RAG')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    load()
  }, [])

  return (
    <AdminPageShell
      title="RAG Dashboard"
      description="Theo dõi chất lượng truy xuất tri thức, feedback và độ trễ của AI."
      actions={
        <button
          onClick={load}
          className="rounded-full bg-primary text-on-primary px-4 py-2 text-sm font-semibold"
        >
          Làm mới
        </button>
      }
    >
      {error && (
        <div className="mb-4 rounded-xl bg-error-container/40 text-error px-4 py-3 text-sm">
          {error}
        </div>
      )}

      {loading ? (
        <p className="text-on-surface-variant">Đang tải...</p>
      ) : (
        <section className="grid gap-4 grid-cols-1 sm:grid-cols-2 lg:grid-cols-4">
          <StatCard
            icon="forum"
            label="RAG traces"
            value={data?.totalTraces.toLocaleString('vi-VN') ?? '0'}
            tone="primary"
          />
          <StatCard
            icon="thumb_down"
            label="Feedback xấu"
            value={data?.negativeFeedbacks.toLocaleString('vi-VN') ?? '0'}
            hint={`Chưa review: ${data?.unreviewedNegativeFeedbacks ?? 0}`}
            tone="tertiary"
          />
          <StatCard
            icon="percent"
            label="Tỷ lệ thumbs down"
            value={`${data?.negativeFeedbackRate ?? 0}%`}
            tone="secondary"
          />
          <StatCard
            icon="hourglass"
            label="P95 latency"
            value={`${data?.p95TotalLatencyMs ?? 0}ms`}
            hint={`Avg: ${data?.avgTotalLatencyMs ?? 0}ms`}
            tone="neutral"
          />
          <StatCard
            icon="search_off"
            label="No-context responses"
            value={data?.noContextResponses.toLocaleString('vi-VN') ?? '0'}
            tone="neutral"
          />
          <StatCard
            icon="medication"
            label="Có thuốc context"
            value={data?.medicationContextResponses.toLocaleString('vi-VN') ?? '0'}
            tone="primary"
          />
          <StatCard
            icon="library_books"
            label="Có knowledge context"
            value={data?.knowledgeContextResponses.toLocaleString('vi-VN') ?? '0'}
            tone="secondary"
          />
          <StatCard
            icon="memory"
            label="Embedding cache"
            value={data?.embeddingCacheSize?.toLocaleString('vi-VN') ?? '0'}
            hint={`Jobs lỗi: ${data?.failedJobs ?? 0} · chờ: ${data?.queuedJobs ?? 0}`}
            tone="neutral"
          />
        </section>
      )}
    </AdminPageShell>
  )
}
