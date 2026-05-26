import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import axios from 'axios'
import { pharmacistApi } from '@/features/pharmacist/pharmacist-api'
import type { PrescriptionDocumentVerification } from '@/features/pharmacist/types'
import { resolveFileUrl } from '@/utils/file-url'
import { PharmacistConsultationsPanel } from '@/features/consultations/PharmacistConsultationsPanel'

function extractApiError(err: unknown, fallback: string) {
  if (axios.isAxiosError(err)) {
    const data = err.response?.data as { title?: string; detail?: string; message?: string; errors?: Record<string, string[]> } | undefined
    if (data?.errors) {
      const first = Object.values(data.errors).flat()[0]
      if (first) return first
    }
    return data?.detail ?? data?.title ?? data?.message ?? err.message ?? fallback
  }
  return fallback
}

type Tab = 'pending' | 'verified' | 'rejected'
type Section = 'prescriptions' | 'consultations'

const TAB_LABELS: Record<Tab, string> = {
  pending: 'Hàng chờ',
  verified: 'Đã duyệt',
  rejected: 'Đã từ chối',
}

const SECTION_LABELS: Record<Section, string> = {
  prescriptions: 'Đơn thuốc',
  consultations: 'Yêu cầu tư vấn',
}

export function PharmacistDashboardPage() {
  const navigate = useNavigate()
  const [section, setSection] = useState<Section>('prescriptions')
  const [tab, setTab] = useState<Tab>('pending')
  const [items, setItems] = useState<PrescriptionDocumentVerification[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  async function load(t: Tab) {
    setLoading(true)
    setError(null)
    try {
      const res =
        t === 'pending'
          ? await pharmacistApi.listPendingDocuments({ page: 1, pageSize: 20 })
          : await pharmacistApi.listHistoryDocuments({ page: 1, pageSize: 20, status: t })
      setItems(res.items)
    } catch (err) {
      setError(extractApiError(err, 'Không tải được danh sách'))
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    if (section === 'prescriptions') load(tab)
  }, [tab, section])

  const emptyMessage =
    tab === 'pending'
      ? { title: 'Không có đơn thuốc chờ duyệt', desc: 'Tất cả tài liệu đã được xử lý.' }
      : tab === 'verified'
        ? { title: 'Chưa có đơn nào được duyệt', desc: 'Lịch sử các đơn bạn (hoặc dược sĩ khác) duyệt sẽ hiển thị ở đây.' }
        : { title: 'Chưa có đơn nào bị từ chối', desc: 'Lịch sử các đơn đã từ chối sẽ hiển thị ở đây.' }

  return (
    <div className="pt-8 pb-24 px-6 md:px-8 max-w-7xl mx-auto animate-in fade-in zoom-in-95 duration-500">
      <header className="mb-6">
        <h1 className="text-4xl md:text-5xl font-extrabold tracking-tight text-on-surface mb-2 font-headline">
          Bảng điều khiển dược sĩ
        </h1>
        <p className="text-on-surface-variant font-medium">
          Mở đơn để đọc file, nhập danh sách thuốc rồi xác minh. Tab Lịch sử để tra cứu các đơn đã xử lý.
        </p>
      </header>

      <div className="mb-6 inline-flex p-1 bg-surface-container rounded-2xl border border-outline-variant/30">
        {(Object.keys(SECTION_LABELS) as Section[]).map((s) => (
          <button
            key={s}
            type="button"
            onClick={() => setSection(s)}
            className={`px-5 py-2 font-semibold text-sm rounded-xl transition-colors ${
              section === s
                ? 'bg-primary text-on-primary shadow-sm'
                : 'text-on-surface-variant hover:text-on-surface'
            }`}
          >
            {SECTION_LABELS[s]}
          </button>
        ))}
      </div>

      {section === 'consultations' && <PharmacistConsultationsPanel />}

      {section === 'prescriptions' && (
      <>
      <div className="mb-6 flex gap-2 border-b border-outline-variant/30">
        {(Object.keys(TAB_LABELS) as Tab[]).map((t) => (
          <button
            key={t}
            type="button"
            onClick={() => setTab(t)}
            className={`px-5 py-2.5 font-semibold text-sm rounded-t-lg border-b-2 -mb-px transition-colors ${
              tab === t
                ? 'border-primary text-primary bg-primary-container/30'
                : 'border-transparent text-on-surface-variant hover:text-on-surface'
            }`}
          >
            {TAB_LABELS[t]}
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
          <span className="material-symbols-outlined text-5xl text-outline mb-3">task_alt</span>
          <h2 className="text-xl font-bold">{emptyMessage.title}</h2>
          <p className="text-on-surface-variant mt-1">{emptyMessage.desc}</p>
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-5">
        {items.map((doc) => {
          const isPending = doc.verificationStatus === 'pending'
          const statusBadge =
            doc.verificationStatus === 'verified'
              ? 'bg-green-100 text-green-800'
              : doc.verificationStatus === 'rejected'
                ? 'bg-error-container text-on-error-container'
                : 'bg-secondary-container text-on-secondary-container'

          return (
            <article
              key={doc.id}
              className="rounded-2xl bg-surface-container-lowest border border-outline-variant/10 p-6 shadow-sm"
            >
              <div className="flex items-start justify-between gap-4 mb-4">
                <div>
                  <h2 className="text-lg font-bold">
                    {doc.prescriptionTitle || `Đơn thuốc #${doc.prescriptionId}`}
                  </h2>
                  <p className="text-sm text-on-surface-variant mt-1">
                    Người dùng: <span className="font-semibold">{doc.userFullName || `#${doc.userId}`}</span>
                  </p>
                  <p className="text-xs text-outline mt-1">
                    {isPending ? 'Upload' : 'Quyết định'}:{' '}
                    {new Date(isPending ? doc.createdAt : doc.updatedAt).toLocaleString('vi-VN')}
                  </p>
                </div>
                <span className={`px-3 py-1.5 rounded-full text-xs font-bold ${statusBadge}`}>
                  {doc.verificationStatus}
                </span>
              </div>

              {doc.notes && !isPending && (
                <p className="text-xs italic text-on-surface-variant mb-3 bg-surface-container-low p-2 rounded-lg">
                  Ghi chú: {doc.notes}
                </p>
              )}

              <div className="rounded-xl bg-surface-container-low p-4 mb-5 border border-outline-variant/20">
                <a
                  href={resolveFileUrl(doc.fileUrl)}
                  target="_blank"
                  rel="noreferrer"
                  className="inline-flex items-center gap-2 text-primary font-semibold hover:underline"
                >
                  <span className="material-symbols-outlined">open_in_new</span>
                  Xem nhanh file đơn thuốc
                </a>
              </div>

              <button
                type="button"
                onClick={() =>
                  navigate(`/pharmacist/prescriptions/${doc.prescriptionId}?documentId=${doc.id}`)
                }
                className="w-full py-3 rounded-xl bg-primary text-on-primary font-bold hover:bg-primary/90 transition-colors shadow-sm"
              >
                {isPending ? 'Mở đơn để xử lý' : 'Xem chi tiết'}
              </button>
            </article>
          )
        })}
      </div>
      </>
      )}
    </div>
  )
}
