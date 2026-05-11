import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import axios from 'axios'
import { prescriptionsApi } from '@/features/prescriptions/prescriptions-api'
import type { PrescriptionListItem, PrescriptionCreateRequest } from '@/features/prescriptions/types'

function extractApiError(err: unknown, fallback: string) {
  if (axios.isAxiosError(err)) {
    const data = err.response?.data as
      | { title?: string; detail?: string; message?: string; errors?: Record<string, string[]> }
      | undefined
    if (data?.errors) {
      const first = Object.values(data.errors).flat()[0]
      if (first) return first
    }
    return data?.detail ?? data?.title ?? data?.message ?? err.message ?? fallback
  }
  return fallback
}

export function PrescriptionsPage() {
  const navigate = useNavigate()
  const [items, setItems] = useState<PrescriptionListItem[]>([])
  const [loading, setLoading] = useState(true)
  const [filter, setFilter] = useState<string>('all')

  // Create modal state
  const [showCreate, setShowCreate] = useState(false)
  const [createForm, setCreateForm] = useState<PrescriptionCreateRequest>({
    title: '',
    doctorNameSnapshot: '',
    prescribedDate: '',
  })
  const [creating, setCreating] = useState(false)
  const [createError, setCreateError] = useState<string | null>(null)

  function loadPrescriptions() {
    let cancelled = false
    setLoading(true)
    prescriptionsApi
      .listMy({ page: 1, pageSize: 50 })
      .then((res) => {
        if (!cancelled) setItems(res.items)
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })
    return () => {
      cancelled = true
    }
  }

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect -- loadPrescriptions fetch-then-setState voi cancelled flag chong race
    return loadPrescriptions()
  }, [])

  async function handleCreate() {
    if (!createForm.title?.trim()) {
      setCreateError('Vui lòng nhập tên đơn thuốc')
      return
    }

    setCreating(true)
    setCreateError(null)

    try {
      const created = await prescriptionsApi.create({
        title: createForm.title?.trim() || null,
        doctorNameSnapshot: createForm.doctorNameSnapshot?.trim() || null,
        prescribedDate: createForm.prescribedDate || null,
      })

      // Navigate to the detail page of the newly created prescription
      navigate(`/prescriptions/${created.id}`)
    } catch (err) {
      setCreateError(extractApiError(err, 'Tạo đơn thuốc thất bại'))
      setCreating(false)
    }
  }

  const filteredItems = items.filter((item) => {
    if (filter === 'all') return true
    return item.verificationStatus === filter
  })

  return (
    <div className="pt-8 pb-24 px-6 md:px-8 max-w-5xl mx-auto animate-in fade-in zoom-in-95 duration-500">
      <header className="mb-8 flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-3xl md:text-4xl font-extrabold tracking-tight text-on-surface mb-2 font-headline">
            Đơn thuốc của tôi
          </h1>
          <p className="text-on-surface-variant font-medium">
            Quản lý đơn thuốc để mua thuốc kê đơn
          </p>
        </div>
        <button
          onClick={() => {
            setShowCreate(true)
            setCreateForm({ title: '', doctorNameSnapshot: '', prescribedDate: '' })
            setCreateError(null)
          }}
          className="px-5 py-2.5 rounded-xl bg-primary text-on-primary font-bold shadow-sm hover:shadow-md transition-shadow flex items-center gap-2 w-fit"
        >
          <span className="material-symbols-outlined text-[20px]">add</span>
          Tạo đơn thuốc
        </button>
      </header>

      <div className="flex gap-2 overflow-x-auto pb-4 mb-4 hide-scrollbar">
        {[
          { id: 'all', label: 'Tất cả' },
          { id: 'pending', label: 'Đang chờ duyệt' },
          { id: 'verified', label: 'Đã xác minh' },
          { id: 'rejected', label: 'Bị từ chối' },
        ].map((f) => (
          <button
            key={f.id}
            onClick={() => setFilter(f.id)}
            className={`whitespace-nowrap px-4 py-2 rounded-full text-sm font-semibold border transition-colors ${
              filter === f.id
                ? 'bg-primary-container text-on-primary-container border-transparent'
                : 'border-outline-variant/50 text-on-surface-variant hover:bg-surface-container-highest'
            }`}
          >
            {f.label}
          </button>
        ))}
      </div>

      {loading && (
        <div className="p-8 text-center text-on-surface-variant">Đang tải...</div>
      )}

      {!loading && filteredItems.length === 0 && (
        <div className="p-10 rounded-2xl bg-surface-container-low text-center">
          <div className="text-4xl mb-3">📋</div>
          <h2 className="text-xl font-bold">Không có đơn thuốc nào</h2>
          <p className="text-on-surface-variant mt-1">Bạn chưa có đơn thuốc nào trong danh sách này.</p>
        </div>
      )}

      <div className="grid gap-4">
        {filteredItems.map((p) => (
          <div
            key={p.id}
            className="flex flex-col sm:flex-row gap-4 p-5 rounded-2xl bg-surface-container-lowest border border-outline-variant/20 shadow-sm"
          >
            <div className="flex-1">
              <h3 className="font-bold text-lg">{p.title || `Đơn thuốc #${p.id}`}</h3>
              <div className="text-sm text-on-surface-variant mt-1">
                Bác sĩ: {p.doctorNameSnapshot || 'Không rõ'} • {p.itemCount} thuốc
              </div>
              <div className="flex gap-2 mt-3">
                <span className="px-2 py-1 rounded-md text-xs font-semibold bg-surface-container-high">
                  {p.status}
                </span>
                <span
                  className={`px-2 py-1 rounded-md text-xs font-semibold ${
                    p.verificationStatus === 'verified'
                      ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
                      : p.verificationStatus === 'rejected'
                      ? 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200'
                      : 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200'
                  }`}
                >
                  {p.verificationStatus}
                </span>
              </div>
            </div>
            <div className="flex sm:flex-col justify-center gap-2">
              <button
                onClick={() => navigate(`/prescriptions/${p.id}`)}
                className="px-4 py-2 rounded-xl border border-outline-variant/50 text-sm font-semibold hover:bg-surface-container-high transition-colors text-center"
              >
                Xem chi tiết
              </button>
            </div>
          </div>
        ))}
      </div>

      {/* Create Prescription Modal */}
      {showCreate && (
        <div className="fixed inset-0 z-[80] bg-black/40 flex items-center justify-center p-4 backdrop-blur-sm animate-in fade-in duration-200">
          <div className="w-full max-w-md rounded-3xl bg-surface p-6 shadow-2xl animate-in zoom-in-95 duration-300">
            <h2 className="text-xl font-bold mb-1">Tạo đơn thuốc mới</h2>
            <p className="text-sm text-on-surface-variant mb-6">
              Nhập thông tin cơ bản, sau đó upload ảnh/PDF đơn thuốc ở trang chi tiết.
            </p>

            <div className="space-y-4">
              <div>
                <label className="block text-sm font-semibold mb-1">
                  Tên đơn thuốc <span className="text-error">*</span>
                </label>
                <input
                  type="text"
                  autoFocus
                  value={createForm.title || ''}
                  onChange={(e) => setCreateForm((f) => ({ ...f, title: e.target.value }))}
                  placeholder="Ví dụ: Đơn thuốc viêm họng tháng 5"
                  className="w-full px-4 py-2.5 rounded-xl bg-surface-container-low border border-outline-variant/20 outline-none focus:ring-2 focus:ring-primary text-sm transition-all"
                />
              </div>
              <div>
                <label className="block text-sm font-semibold mb-1">Tên bác sĩ kê đơn</label>
                <input
                  type="text"
                  value={createForm.doctorNameSnapshot || ''}
                  onChange={(e) => setCreateForm((f) => ({ ...f, doctorNameSnapshot: e.target.value }))}
                  placeholder="Ví dụ: BS. Nguyễn Văn A"
                  className="w-full px-4 py-2.5 rounded-xl bg-surface-container-low border border-outline-variant/20 outline-none focus:ring-2 focus:ring-primary text-sm transition-all"
                />
              </div>
              <div>
                <label className="block text-sm font-semibold mb-1">Ngày kê đơn</label>
                <input
                  type="date"
                  value={createForm.prescribedDate || ''}
                  onChange={(e) => setCreateForm((f) => ({ ...f, prescribedDate: e.target.value }))}
                  className="w-full px-4 py-2.5 rounded-xl bg-surface-container-low border border-outline-variant/20 outline-none focus:ring-2 focus:ring-primary text-sm transition-all"
                />
              </div>
            </div>

            {createError && (
              <div className="mt-4 p-3 rounded-xl bg-error-container text-on-error-container text-sm font-medium">
                {createError}
              </div>
            )}

            <div className="mt-6 flex justify-end gap-3">
              <button
                type="button"
                onClick={() => setShowCreate(false)}
                disabled={creating}
                className="px-5 py-2.5 rounded-xl border border-outline-variant/40 font-semibold hover:bg-surface-container-high transition-colors disabled:opacity-50"
              >
                Hủy
              </button>
              <button
                type="button"
                onClick={handleCreate}
                disabled={creating}
                className="px-5 py-2.5 rounded-xl bg-primary text-on-primary font-semibold disabled:opacity-50 hover:bg-primary/90 transition-colors shadow-sm"
              >
                {creating ? 'Đang tạo...' : 'Tạo đơn thuốc'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
