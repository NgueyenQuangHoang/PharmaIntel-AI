// =============================================================================
// PharmacistConsultationsPanel - Tab "Yeu cau tu van" trong /pharmacist.
// Chuc nang: list yeu cau theo trang thai, cho phep duoc si Chap nhan / Tu choi /
// Hoan thanh. Sub-tabs: Hang cho / Da nhan / Lich su.
// =============================================================================
import { useCallback, useEffect, useState } from 'react'
import axios from 'axios'
import { consultationsApi } from './consultations-api'
import type { Consultation, ConsultationStatus } from './types'

type SubTab = 'pending' | 'accepted' | 'history'

const SUB_TAB_LABELS: Record<SubTab, string> = {
  pending: 'Hàng chờ',
  accepted: 'Đã nhận',
  history: 'Lịch sử',
}

function extractApiError(err: unknown, fallback: string) {
  if (axios.isAxiosError(err)) {
    const data = err.response?.data as { title?: string; detail?: string; message?: string } | undefined
    return data?.detail ?? data?.title ?? data?.message ?? err.message ?? fallback
  }
  return fallback
}

function formatDateTime(iso: string) {
  return new Date(iso).toLocaleString('vi-VN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  })
}

function statusBadge(status: ConsultationStatus) {
  switch (status) {
    case 'accepted':
      return 'bg-green-100 text-green-800'
    case 'rejected':
    case 'cancelled':
      return 'bg-error-container text-on-error-container'
    case 'completed':
      return 'bg-primary-container text-on-primary-container'
    default:
      return 'bg-secondary-container text-on-secondary-container'
  }
}

const STATUS_LABEL: Record<ConsultationStatus, string> = {
  pending: 'Chờ duyệt',
  accepted: 'Đã nhận',
  rejected: 'Đã từ chối',
  completed: 'Hoàn thành',
  cancelled: 'Đã hủy',
}

export function PharmacistConsultationsPanel() {
  const [subTab, setSubTab] = useState<SubTab>('pending')
  const [items, setItems] = useState<Consultation[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [actionId, setActionId] = useState<number | null>(null)

  const load = useCallback(async (tab: SubTab) => {
    setLoading(true)
    setError(null)
    try {
      // 'history' = lay tat ca, loc client-side; pending/accepted goi voi status.
      const res = tab === 'history'
        ? await consultationsApi.listForPharmacist({ page: 1, pageSize: 50 })
        : await consultationsApi.listForPharmacist({ page: 1, pageSize: 50, status: tab })
      const filtered = tab === 'history'
        ? res.items.filter((c) => c.status === 'rejected' || c.status === 'completed' || c.status === 'cancelled')
        : res.items
      setItems(filtered)
    } catch (err) {
      setError(extractApiError(err, 'Không tải được danh sách yêu cầu tư vấn'))
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    void load(subTab)
  }, [subTab, load])

  async function updateStatus(id: number, status: ConsultationStatus) {
    setActionId(id)
    try {
      await consultationsApi.updateStatus(id, { status })
      await load(subTab)
    } catch (err) {
      setError(extractApiError(err, 'Cập nhật trạng thái thất bại'))
    } finally {
      setActionId(null)
    }
  }

  return (
    <div>
      <div className="mb-6 flex gap-2 border-b border-outline-variant/30">
        {(Object.keys(SUB_TAB_LABELS) as SubTab[]).map((t) => (
          <button
            key={t}
            type="button"
            onClick={() => setSubTab(t)}
            className={`px-5 py-2.5 font-semibold text-sm rounded-t-lg border-b-2 -mb-px transition-colors ${
              subTab === t
                ? 'border-primary text-primary bg-primary-container/30'
                : 'border-transparent text-on-surface-variant hover:text-on-surface'
            }`}
          >
            {SUB_TAB_LABELS[t]}
          </button>
        ))}
      </div>

      {error && (
        <div className="mb-4 p-3 rounded-xl bg-error-container text-on-error-container text-sm font-medium">
          {error}
        </div>
      )}

      {loading && (
        <div className="p-8 rounded-xl bg-surface-container-low text-on-surface-variant">
          Đang tải dữ liệu...
        </div>
      )}

      {!loading && items.length === 0 && (
        <div className="p-10 rounded-2xl bg-surface-container-low text-center border border-outline-variant/20 shadow-sm">
          <span className="material-symbols-outlined text-5xl text-outline mb-3">event_busy</span>
          <h2 className="text-xl font-bold">Không có yêu cầu tư vấn nào</h2>
          <p className="text-on-surface-variant mt-1">Khi có người dùng đặt lịch, yêu cầu sẽ hiển thị ở đây.</p>
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-5">
        {items.map((c) => {
          const busy = actionId === c.id
          return (
            <article
              key={c.id}
              className="rounded-2xl bg-surface-container-lowest border border-outline-variant/10 p-6 shadow-sm"
            >
              <div className="flex items-start justify-between gap-4 mb-3">
                <div>
                  <h3 className="text-lg font-bold">{c.userFullName || `Người dùng #${c.userId}`}</h3>
                  {c.userEmail && (
                    <p className="text-xs text-on-surface-variant mt-0.5 flex items-center gap-1">
                      <span className="material-symbols-outlined text-[14px]">mail</span>
                      {c.userEmail}
                    </p>
                  )}
                  {c.userPhoneNumber && (
                    <p className="text-xs text-on-surface-variant mt-0.5 flex items-center gap-1">
                      <span className="material-symbols-outlined text-[14px]">call</span>
                      <a href={`tel:${c.userPhoneNumber}`} className="hover:text-primary hover:underline">
                        {c.userPhoneNumber}
                      </a>
                    </p>
                  )}
                  <p className="text-sm text-on-surface-variant mt-2 flex items-center gap-1">
                    <span className="material-symbols-outlined text-[18px] text-primary">calendar_clock</span>
                    <span className="font-semibold text-on-surface">{formatDateTime(c.scheduledAt)}</span>
                  </p>
                </div>
                <span className={`px-3 py-1.5 rounded-full text-xs font-bold ${statusBadge(c.status)}`}>
                  {STATUS_LABEL[c.status]}
                </span>
              </div>

              {c.note && (
                <p className="text-sm text-on-surface-variant mb-4 bg-surface-container-low p-3 rounded-lg">
                  <span className="font-semibold text-on-surface">Vấn đề: </span>
                  {c.note}
                </p>
              )}

              {c.responseNote && (
                <p className="text-xs italic text-on-surface-variant mb-4 bg-surface-container-low p-2 rounded-lg">
                  Ghi chú phản hồi: {c.responseNote}
                </p>
              )}

              {c.status === 'pending' && (
                <div className="flex gap-2">
                  <button
                    type="button"
                    disabled={busy}
                    onClick={() => updateStatus(c.id, 'accepted')}
                    className="flex-1 py-2.5 rounded-xl bg-primary text-on-primary font-bold hover:bg-primary/90 disabled:opacity-50 transition-colors shadow-sm"
                  >
                    {busy ? 'Đang xử lý...' : 'Chấp nhận'}
                  </button>
                  <button
                    type="button"
                    disabled={busy}
                    onClick={() => updateStatus(c.id, 'rejected')}
                    className="flex-1 py-2.5 rounded-xl bg-error-container text-on-error-container font-bold hover:opacity-90 disabled:opacity-50 transition-colors"
                  >
                    Từ chối
                  </button>
                </div>
              )}

              {c.status === 'accepted' && (
                <button
                  type="button"
                  disabled={busy}
                  onClick={() => updateStatus(c.id, 'completed')}
                  className="w-full py-2.5 rounded-xl bg-primary text-on-primary font-bold hover:bg-primary/90 disabled:opacity-50 transition-colors shadow-sm"
                >
                  {busy ? 'Đang xử lý...' : 'Đánh dấu hoàn thành'}
                </button>
              )}
            </article>
          )
        })}
      </div>
    </div>
  )
}
