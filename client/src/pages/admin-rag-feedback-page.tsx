import { useEffect, useState } from 'react'
import { AdminPageShell } from '@/features/admin/components/AdminPageShell'
import { ragApi, type AiFeedback } from '@/features/rag/rag-api'

export function AdminRagFeedbackPage() {
  const [items, setItems] = useState<AiFeedback[]>([])
  const [isReviewed, setIsReviewed] = useState<boolean | undefined>(false)
  const [rating, setRating] = useState<'thumbs_up' | 'thumbs_down' | undefined>('thumbs_down')
  const [loading, setLoading] = useState(true)

  const load = async () => {
    setLoading(true)
    try {
      setItems(await ragApi.listFeedback({ isReviewed, rating }))
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    load()
  }, [isReviewed, rating])

  const markReviewed = async (id: number) => {
    const note = prompt('Ghi chú admin khi review feedback:') ?? ''
    await ragApi.reviewFeedback(id, note)
    await load()
  }

  return (
    <AdminPageShell
      title="Feedback AI"
      description="Review các phản hồi thumbs down để cải thiện RAG và prompt."
    >
      <div className="mb-4 flex flex-wrap gap-2">
        <select
          value={rating ?? ''}
          onChange={(e) => setRating((e.target.value || undefined) as any)}
          className="rounded-full border border-outline-variant/40 bg-surface px-4 py-2 text-sm"
        >
          <option value="">Tất cả rating</option>
          <option value="thumbs_down">Thumbs down</option>
          <option value="thumbs_up">Thumbs up</option>
        </select>

        <select
          value={isReviewed === undefined ? '' : String(isReviewed)}
          onChange={(e) =>
            setIsReviewed(e.target.value === '' ? undefined : e.target.value === 'true')
          }
          className="rounded-full border border-outline-variant/40 bg-surface px-4 py-2 text-sm"
        >
          <option value="">Tất cả</option>
          <option value="false">Chưa review</option>
          <option value="true">Đã review</option>
        </select>
      </div>

      <section className="rounded-2xl border border-outline-variant/40 bg-surface-container/60 overflow-hidden">
        {loading ? (
          <p className="p-5 text-on-surface-variant">Đang tải...</p>
        ) : items.length === 0 ? (
          <p className="p-5 text-on-surface-variant">Chưa có feedback phù hợp.</p>
        ) : (
          <div className="divide-y divide-outline-variant/30">
            {items.map((f) => (
              <article key={f.id} className="p-5">
                <div className="flex items-start justify-between gap-4">
                  <div>
                    <p className="font-bold">
                      {f.rating === 'thumbs_down' ? '👎 Không hài lòng' : '👍 Hài lòng'}
                    </p>
                    <p className="text-sm text-on-surface-variant mt-1">
                      Reason: {f.reasonType ?? '—'} · User #{f.userId}
                    </p>
                    <p className="text-xs text-on-surface-variant mt-1">
                      Session: {f.diagnosticSessionId ?? '—'} · Message:{' '}
                      {f.diagnosticMessageId ?? '—'} · Trace: {f.ragTraceId ?? '—'}
                    </p>
                  </div>

                  {!f.isReviewed ? (
                    <button
                      onClick={() => markReviewed(f.id)}
                      className="rounded-full bg-primary text-on-primary px-4 py-2 text-xs font-semibold"
                    >
                      Mark reviewed
                    </button>
                  ) : (
                    <span className="rounded-full bg-primary-container/40 text-primary px-3 py-1 text-xs font-semibold">
                      Đã review
                    </span>
                  )}
                </div>

                {f.comment && (
                  <div className="mt-3 rounded-xl bg-surface-container-lowest border border-outline-variant/30 p-3 text-sm">
                    {f.comment}
                  </div>
                )}

                {f.adminNote && (
                  <p className="mt-2 text-xs text-on-surface-variant">
                    Admin note: {f.adminNote}
                  </p>
                )}
              </article>
            ))}
          </div>
        )}
      </section>
    </AdminPageShell>
  )
}
