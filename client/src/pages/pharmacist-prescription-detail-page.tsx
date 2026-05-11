import { useEffect, useState } from 'react'
import { useNavigate, useParams, useSearchParams } from 'react-router-dom'
import axios from 'axios'
import { pharmacistApi } from '@/features/pharmacist/pharmacist-api'
import type {
  Prescription,
  PrescriptionItem,
  PrescriptionItemCreateRequest,
} from '@/features/prescriptions/types'
import { MedicationCombobox } from '@/features/prescriptions/components/MedicationCombobox'
import { parseDurationDays, previewReminderSlots } from '@/features/prescriptions/utils/reminderSlots'
import { resolveFileUrl } from '@/utils/file-url'

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

const emptyForm: PrescriptionItemCreateRequest = {
  medicationId: null,
  medicationName: '',
  dosage: '',
  frequency: '',
  duration: '',
}

export function PharmacistPrescriptionDetailPage() {
  const { id } = useParams<{ id: string }>()
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const prescriptionId = Number(id)
  // documentId optional: khi duoc si bam vao card cu the o dashboard, query nay
  // pin dung file ho click. Neu thieu (vd vao tu lich su) thi fallback file pending dau tien.
  const targetDocumentId = Number(searchParams.get('documentId')) || null

  const [prescription, setPrescription] = useState<Prescription | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)

  const [form, setForm] = useState<PrescriptionItemCreateRequest>(emptyForm)
  const [editingId, setEditingId] = useState<number | null>(null)

  const [rejecting, setRejecting] = useState(false)
  const [rejectNotes, setRejectNotes] = useState('')

  async function reload() {
    try {
      const p = await pharmacistApi.getPrescription(prescriptionId)
      setPrescription(p)
    } catch (err) {
      setError(extractApiError(err, 'Không tải được chi tiết đơn thuốc'))
    }
  }

  useEffect(() => {
    if (!prescriptionId) return
    setLoading(true)
    reload().finally(() => setLoading(false))
  }, [prescriptionId])

  function resetForm() {
    setForm(emptyForm)
    setEditingId(null)
  }

  function loadIntoForm(item: PrescriptionItem) {
    setEditingId(item.id)
    setForm({
      medicationId: item.medicationId,
      medicationName: item.medicationName,
      dosage: item.dosage,
      frequency: item.frequency,
      duration: item.duration,
    })
  }

  async function submitItem() {
    if (!form.medicationName?.trim()) {
      setError('Vui lòng nhập tên thuốc')
      return
    }
    setBusy(true)
    setError(null)
    try {
      if (editingId) {
        await pharmacistApi.updatePrescriptionItem(editingId, form)
      } else {
        await pharmacistApi.addPrescriptionItem(prescriptionId, form)
      }
      await reload()
      resetForm()
    } catch (err) {
      setError(extractApiError(err, 'Không lưu được mục thuốc'))
    } finally {
      setBusy(false)
    }
  }

  async function removeItem(itemId: number) {
    if (!confirm('Xóa mục thuốc này?')) return
    setBusy(true)
    setError(null)
    try {
      await pharmacistApi.removePrescriptionItem(itemId)
      await reload()
      if (editingId === itemId) resetForm()
    } catch (err) {
      setError(extractApiError(err, 'Không xóa được'))
    } finally {
      setBusy(false)
    }
  }

  // Resolve dung file dang xu ly:
  //  1. Neu URL co ?documentId= va file do tim thay + dang pending -> dung file do.
  //  2. Khong thi fallback file pending dau tien (giu hanh vi cu khi mo tu cho khong co context).
  function resolveTargetPendingDoc() {
    if (!prescription?.documents) return undefined
    if (targetDocumentId) {
      const exact = prescription.documents.find((d) => d.id === targetDocumentId)
      if (exact && exact.verificationStatus === 'pending') return exact
    }
    return prescription.documents.find((d) => d.verificationStatus === 'pending')
  }

  async function verify() {
    if (!prescription || prescription.items.length === 0) return
    const pending = resolveTargetPendingDoc()
    if (!pending) {
      setError('Không tìm thấy file đang chờ xác minh')
      return
    }
    setBusy(true)
    setError(null)
    try {
      await pharmacistApi.verifyDocument(pending.id, { notes: 'Đơn thuốc hợp lệ.' })
      navigate('/pharmacist')
    } catch (err) {
      setError(extractApiError(err, 'Xác minh thất bại'))
    } finally {
      setBusy(false)
    }
  }

  async function reject() {
    if (!prescription) return
    const pending = resolveTargetPendingDoc()
    if (!pending) return
    if (rejectNotes.trim().length < 5) {
      setError('Vui lòng nhập lý do từ chối tối thiểu 5 ký tự')
      return
    }
    setBusy(true)
    setError(null)
    try {
      await pharmacistApi.rejectDocument(pending.id, { notes: rejectNotes.trim() })
      navigate('/pharmacist')
    } catch (err) {
      setError(extractApiError(err, 'Từ chối thất bại'))
    } finally {
      setBusy(false)
    }
  }

  if (loading) return <div className="p-8 text-center mt-10">Đang tải...</div>
  if (!prescription)
    return <div className="p-8 text-center text-error mt-10">{error || 'Không tìm thấy đơn thuốc'}</div>

  const pendingDoc = resolveTargetPendingDoc()
  const canVerify = !!pendingDoc && prescription.items.length > 0
  // Dong bo voi backend (PharmacistPrescriptionItemService): chi cho sua khi co
  // document dang 'pending'. Cac state khac (verified, rejected, hoac don rong
  // chua upload file) deu chi xem.
  const hasAnyPendingDoc = !!prescription.documents?.some((d) => d.verificationStatus === 'pending')
  const isLocked = !hasAnyPendingDoc

  return (
    <div className="pt-8 pb-24 px-6 md:px-8 max-w-7xl mx-auto animate-in fade-in zoom-in-95 duration-500">
      <button
        onClick={() => navigate('/pharmacist')}
        className="mb-6 flex items-center gap-1 text-sm font-semibold text-on-surface-variant hover:text-primary"
      >
        <span className="material-symbols-outlined text-[18px]">arrow_back</span>
        Quay lại hàng chờ
      </button>

      <header className="mb-6">
        <h1 className="text-3xl font-extrabold tracking-tight mb-2">
          {prescription.title || `Đơn thuốc #${prescription.id}`}
        </h1>
        <div className="flex flex-wrap gap-3 text-sm">
          <span className="px-3 py-1 rounded-full bg-surface-container-high font-semibold">
            Người dùng: {prescription.userFullName || `#${prescription.userId}`}
          </span>
          <span className="px-3 py-1 rounded-full bg-surface-container-high font-semibold">
            Bác sĩ: {prescription.doctorNameSnapshot || 'Không rõ'}
          </span>
          <span className="px-3 py-1 rounded-full bg-primary-container text-on-primary-container font-semibold">
            Xác minh: {prescription.verificationStatus}
          </span>
        </div>
      </header>

      {error && (
        <div className="mb-4 p-3 rounded-xl bg-error-container text-on-error-container text-sm font-medium">
          {error}
        </div>
      )}

      {isLocked && (
        <div className="mb-4 p-3 rounded-xl bg-amber-50 border border-amber-200 text-amber-900 text-sm">
          {prescription.verificationStatus === 'verified' || prescription.verificationStatus === 'rejected' ? (
            <>
              <span className="font-semibold">Đơn đã chốt</span> ({prescription.verificationStatus}).
              Chế độ chỉ xem. Nếu user upload file mới, đơn sẽ trở lại trạng thái chờ xác minh.
            </>
          ) : (
            <>
              <span className="font-semibold">Chưa có file đơn chờ xác minh</span> — không thể nhập items.
              Hãy đợi user upload file đơn bác sĩ rồi mở lại.
            </>
          )}
        </div>
      )}

      <div className="grid gap-6 lg:grid-cols-2">
        <section className="p-5 rounded-2xl bg-surface-container-lowest border border-outline-variant/20 shadow-sm">
          <h2 className="font-bold text-lg mb-4 flex items-center gap-2">
            <span className="material-symbols-outlined text-primary">image</span>
            File đơn bác sĩ
          </h2>
          {prescription.documents && prescription.documents.length > 0 ? (
            <div className="space-y-3">
              {prescription.documents.map((doc) => {
                const url = resolveFileUrl(doc.fileUrl)
                const isPdf = doc.fileUrl.toLowerCase().endsWith('.pdf')
                const isTarget = pendingDoc?.id === doc.id
                return (
                  <div
                    key={doc.id}
                    className={`rounded-xl border p-3 bg-surface-container-low ${
                      isTarget ? 'border-primary ring-2 ring-primary/30' : 'border-outline-variant/20'
                    }`}
                  >
                    <div className="flex items-center justify-between mb-2 gap-2">
                      <span className="text-xs font-semibold">
                        File #{doc.id} — {doc.verificationStatus}
                        {isTarget && (
                          <span className="ml-2 text-[10px] px-1.5 py-0.5 rounded bg-primary text-on-primary">
                            Đang xử lý
                          </span>
                        )}
                      </span>
                      <a
                        href={url}
                        target="_blank"
                        rel="noreferrer"
                        className="text-xs font-semibold text-primary hover:underline"
                      >
                        Mở tab mới
                      </a>
                    </div>
                    {isPdf ? (
                      <iframe
                        src={url}
                        className="w-full h-[600px] rounded-lg bg-white"
                        title={`pdf-${doc.id}`}
                      />
                    ) : (
                      <img src={url} alt={`doc-${doc.id}`} className="w-full rounded-lg" />
                    )}
                  </div>
                )
              })}
            </div>
          ) : (
            <p className="text-sm text-on-surface-variant">Chưa có file đính kèm.</p>
          )}
        </section>

        <section className="p-5 rounded-2xl bg-surface-container-lowest border border-outline-variant/20 shadow-sm">
          <h2 className="font-bold text-lg mb-4 flex items-center gap-2">
            <span className="material-symbols-outlined text-primary">medication</span>
            Danh sách thuốc ({prescription.items.length})
          </h2>

          <div className="space-y-2 mb-5">
            {prescription.items.map((item) => (
              <div
                key={item.id}
                className="flex items-start justify-between gap-3 p-3 rounded-xl border border-outline-variant/20 bg-surface-container-low"
              >
                <div className="text-sm flex-1">
                  <div className="font-bold">{item.medicationName}</div>
                  <div className="text-xs text-on-surface-variant mt-1">
                    {[item.dosage, item.frequency, item.duration].filter(Boolean).join(' • ') || '—'}
                  </div>
                </div>
                {!isLocked && (
                  <div className="flex gap-2 shrink-0">
                    <button
                      type="button"
                      onClick={() => loadIntoForm(item)}
                      className="text-xs font-semibold text-primary hover:underline"
                    >
                      Sửa
                    </button>
                    <button
                      type="button"
                      onClick={() => removeItem(item.id)}
                      className="text-xs font-semibold text-error hover:underline"
                    >
                      Xóa
                    </button>
                  </div>
                )}
              </div>
            ))}
            {prescription.items.length === 0 && (
              <p className="text-sm text-on-surface-variant italic">
                Chưa có thuốc nào — vui lòng đọc file và nhập thuốc trước khi xác minh.
              </p>
            )}
          </div>

          {!isLocked && (
          <div className="p-4 rounded-xl bg-surface-container-low border border-outline-variant/20 space-y-3">
            <div className="font-semibold text-sm">
              {editingId ? `Sửa mục #${editingId}` : 'Thêm thuốc'}
            </div>
            <MedicationCombobox
              value={{
                medicationId: form.medicationId ?? null,
                medicationName: form.medicationName ?? '',
              }}
              onChange={(next) =>
                setForm((f) => ({
                  ...f,
                  medicationId: next.medicationId,
                  medicationName: next.medicationName,
                }))
              }
              placeholder="Tên thuốc (VD: Paracetamol 500mg)"
            />
            <div className="grid grid-cols-3 gap-2">
              <input
                value={form.dosage ?? ''}
                onChange={(e) => setForm((f) => ({ ...f, dosage: e.target.value }))}
                placeholder="Liều (1 viên)"
                className="rounded-lg bg-surface px-3 py-2 text-sm border border-outline-variant/30"
              />
              <input
                value={form.frequency ?? ''}
                onChange={(e) => setForm((f) => ({ ...f, frequency: e.target.value }))}
                placeholder="Tần suất (3 lần/ngày)"
                className="rounded-lg bg-surface px-3 py-2 text-sm border border-outline-variant/30"
              />
              <input
                value={form.duration ?? ''}
                onChange={(e) => setForm((f) => ({ ...f, duration: e.target.value }))}
                placeholder="Thời gian (5 ngày)"
                className="rounded-lg bg-surface px-3 py-2 text-sm border border-outline-variant/30"
              />
            </div>
            <div className="flex gap-2">
              <button
                type="button"
                disabled={busy}
                onClick={submitItem}
                className="flex-1 py-2 rounded-lg bg-primary text-on-primary font-semibold text-sm disabled:opacity-50"
              >
                {editingId ? 'Cập nhật' : 'Thêm'}
              </button>
              {editingId && (
                <button
                  type="button"
                  onClick={resetForm}
                  className="px-4 py-2 rounded-lg border border-outline-variant/40 font-semibold text-sm"
                >
                  Hủy
                </button>
              )}
            </div>
            {(() => {
              const preview = previewReminderSlots(form.frequency)
              const days = parseDurationDays(form.duration)
              const today = new Date()
              const endDateLabel = days
                ? new Date(today.getTime() + (days - 1) * 86400000).toLocaleDateString('vi-VN')
                : null
              return (
                <div
                  className={`text-[11px] rounded-lg px-3 py-2 ${
                    preview.matched
                      ? 'bg-primary-container/40 text-on-primary-container'
                      : 'bg-amber-50 text-amber-900 border border-amber-200'
                  }`}
                >
                  {preview.matched ? (
                    <>
                      <span className="font-semibold">Hệ thống sẽ tạo lịch nhắc:</span>{' '}
                      {preview.slots.join(', ')}{' '}
                      <span className="text-on-surface-variant">(mỗi ngày)</span>
                      <div className="mt-1">
                        <span className="font-semibold">Lịch chạy:</span>{' '}
                        {today.toLocaleDateString('vi-VN')}{' → '}
                        {endDateLabel ?? (
                          <span className="italic">đến khi dược sĩ/user dừng thủ công</span>
                        )}
                        {endDateLabel && <span> ({days} ngày)</span>}
                      </div>
                    </>
                  ) : (
                    <>
                      <span className="font-semibold">Không khớp pattern "N lần/ngày"</span> — sẽ tạo
                      1 lịch mặc định {preview.slots[0]} mỗi ngày. Hãy ghi rõ "1 lần/ngày", "2 lần/ngày"…
                      nếu muốn nhiều slot.
                    </>
                  )}
                </div>
              )
            })()}
          </div>
          )}
        </section>
      </div>

      {!isLocked && (
        <div className="mt-8 flex flex-wrap gap-3 justify-end">
          <button
            type="button"
            disabled={busy || !pendingDoc}
            onClick={() => setRejecting(true)}
            className="px-6 py-3 rounded-xl bg-error-container text-on-error-container font-bold disabled:opacity-50"
          >
            Từ chối đơn
          </button>
          <button
            type="button"
            disabled={busy || !canVerify}
            onClick={verify}
            className="px-6 py-3 rounded-xl bg-primary text-on-primary font-bold disabled:opacity-50"
            title={!canVerify ? 'Phải có file pending + ít nhất 1 thuốc' : ''}
          >
            Xác minh đơn
          </button>
        </div>
      )}

      {rejecting && (
        <div className="fixed inset-0 z-[80] bg-black/40 flex items-center justify-center p-4 backdrop-blur-sm">
          <div className="w-full max-w-md rounded-3xl bg-surface p-6 shadow-2xl">
            <h2 className="text-xl font-bold mb-2">Từ chối đơn thuốc</h2>
            <p className="text-sm text-on-surface-variant mb-4">
              Nhập lý do để người dùng biết cần upload lại gì.
            </p>
            <textarea
              value={rejectNotes}
              onChange={(e) => setRejectNotes(e.target.value)}
              className="w-full min-h-28 rounded-xl bg-surface-container-low p-4 outline-none focus:ring-2 focus:ring-primary border border-outline-variant/20 text-sm"
              placeholder="Ví dụ: Ảnh mờ, không đọc được tên thuốc..."
              autoFocus
            />
            <div className="mt-6 flex justify-end gap-3">
              <button
                type="button"
                onClick={() => setRejecting(false)}
                className="px-5 py-2.5 rounded-xl border border-outline-variant/40 font-semibold"
              >
                Hủy
              </button>
              <button
                type="button"
                onClick={reject}
                disabled={busy}
                className="px-5 py-2.5 rounded-xl bg-error text-on-error font-semibold disabled:opacity-50"
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
