import { useEffect, useState } from 'react'
import axios from 'axios'
import { pharmacistApi } from '@/features/pharmacist/pharmacist-api'
import type { PrescriptionDocumentVerification } from '@/features/pharmacist/types'
import { resolveFileUrl } from '@/utils/file-url'

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

export function PharmacistDashboardPage() {
  const [items, setItems] = useState<PrescriptionDocumentVerification[]>([])
  const [loading, setLoading] = useState(true)
  const [busyId, setBusyId] = useState<number | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [rejecting, setRejecting] = useState<PrescriptionDocumentVerification | null>(null)
  const [rejectNotes, setRejectNotes] = useState('')

  async function load() {
    setLoading(true)
    setError(null)
    try {
      const res = await pharmacistApi.listPendingDocuments({ page: 1, pageSize: 20 })
      setItems(res.items)
    } catch (err) {
      setError(extractApiError(err, 'Không tải được hàng chờ xác minh'))
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    load()
  }, [])

  async function verify(id: number) {
    setBusyId(id)
    setError(null)
    try {
      await pharmacistApi.verifyDocument(id, { notes: 'Đơn thuốc hợp lệ.' })
      setItems((prev) => prev.filter((x) => x.id !== id))
    } catch (err) {
      setError(extractApiError(err, 'Xác minh thất bại'))
    } finally {
      setBusyId(null)
    }
  }

  async function reject() {
    if (!rejecting) return
    if (rejectNotes.trim().length < 5) {
      setError('Vui lòng nhập lý do từ chối tối thiểu 5 ký tự')
      return
    }

    setBusyId(rejecting.id)
    setError(null)

    try {
      await pharmacistApi.rejectDocument(rejecting.id, { notes: rejectNotes.trim() })
      setItems((prev) => prev.filter((x) => x.id !== rejecting.id))
      setRejecting(null)
      setRejectNotes('')
    } catch (err) {
      setError(extractApiError(err, 'Từ chối thất bại'))
    } finally {
      setBusyId(null)
    }
  }

  return (
    <div className="pt-8 pb-24 px-6 md:px-8 max-w-7xl mx-auto animate-in fade-in zoom-in-95 duration-500">
      <header className="mb-8">
        <h1 className="text-4xl md:text-5xl font-extrabold tracking-tight text-on-surface mb-2 font-headline">
          Hàng chờ xác minh
        </h1>
        <p className="text-on-surface-variant font-medium">
          Duyệt ảnh/PDF đơn thuốc do người dùng upload.
        </p>
      </header>

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
          <h2 className="text-xl font-bold">Không có đơn thuốc chờ duyệt</h2>
          <p className="text-on-surface-variant mt-1">Tất cả tài liệu đã được xử lý.</p>
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-5">
        {items.map((doc) => (
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
                  Upload: {new Date(doc.createdAt).toLocaleString('vi-VN')}
                </p>
              </div>
              <span className="px-3 py-1.5 rounded-full text-xs font-bold bg-secondary-container text-on-secondary-container">
                Pending
              </span>
            </div>

            <div className="rounded-xl bg-surface-container-low p-4 mb-5 border border-outline-variant/20">
              <a
                href={resolveFileUrl(doc.fileUrl)}
                target="_blank"
                rel="noreferrer"
                className="inline-flex items-center gap-2 text-primary font-semibold hover:underline"
              >
                <span className="material-symbols-outlined">open_in_new</span>
                Mở file đơn thuốc
              </a>
            </div>

            <div className="flex gap-3 mt-auto">
              <button
                type="button"
                disabled={busyId === doc.id}
                onClick={() => verify(doc.id)}
                className="flex-1 py-3 rounded-xl bg-primary text-on-primary font-bold disabled:opacity-50 hover:bg-primary/90 transition-colors shadow-sm"
              >
                Xác minh
              </button>
              <button
                type="button"
                disabled={busyId === doc.id}
                onClick={() => {
                  setRejecting(doc)
                  setRejectNotes('')
                }}
                className="flex-1 py-3 rounded-xl bg-error-container text-on-error-container font-bold disabled:opacity-50 hover:bg-error-container/80 transition-colors shadow-sm"
              >
                Từ chối
              </button>
            </div>
          </article>
        ))}
      </div>

      {rejecting && (
        <div className="fixed inset-0 z-[80] bg-black/40 flex items-center justify-center p-4 backdrop-blur-sm animate-in fade-in duration-200">
          <div className="w-full max-w-md rounded-3xl bg-surface p-6 shadow-2xl animate-in zoom-in-95 duration-300">
            <h2 className="text-xl font-bold mb-2">Từ chối đơn thuốc</h2>
            <p className="text-sm text-on-surface-variant mb-4">
              Nhập lý do để người dùng biết cần upload lại gì.
            </p>
            <textarea
              value={rejectNotes}
              onChange={(e) => setRejectNotes(e.target.value)}
              className="w-full min-h-28 rounded-xl bg-surface-container-low p-4 outline-none focus:ring-2 focus:ring-primary border border-outline-variant/20 transition-all text-sm"
              placeholder="Ví dụ: Ảnh bị mờ, không đọc được tên thuốc..."
              autoFocus
            />
            <div className="mt-6 flex justify-end gap-3">
              <button
                type="button"
                onClick={() => setRejecting(null)}
                className="px-5 py-2.5 rounded-xl border border-outline-variant/40 font-semibold hover:bg-surface-container-high transition-colors"
              >
                Hủy
              </button>
              <button
                type="button"
                onClick={reject}
                disabled={busyId === rejecting.id}
                className="px-5 py-2.5 rounded-xl bg-error text-on-error font-semibold disabled:opacity-50 hover:bg-error/90 transition-colors shadow-sm"
              >
                Xác nhận từ chối
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
