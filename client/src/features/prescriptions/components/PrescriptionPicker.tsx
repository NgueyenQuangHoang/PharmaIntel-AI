import type { PrescriptionListItem } from '../types'

function statusLabel(status: string) {
  switch (status) {
    case 'verified': return 'Đã xác minh'
    case 'pending': return 'Đang chờ duyệt'
    case 'rejected': return 'Bị từ chối'
    default: return status
  }
}

export function PrescriptionPicker({
  prescriptions,
  selectedId,
  onSelect,
  loading,
}: {
  prescriptions: PrescriptionListItem[]
  selectedId: number | null
  onSelect: (id: number) => void
  loading?: boolean
}) {
  if (loading) {
    return (
      <div className="p-5 rounded-xl bg-surface-container-low text-on-surface-variant animate-pulse">
        Đang tải đơn thuốc đã xác minh...
      </div>
    )
  }

  if (prescriptions.length === 0) {
    return (
      <div className="p-5 rounded-xl bg-error-container/40 text-on-error-container border border-error/20">
        <div className="font-bold mb-1">Chưa có đơn thuốc đã xác minh</div>
        <p className="text-sm">
          Bạn cần tạo đơn thuốc, upload ảnh/PDF đơn thuốc và chờ dược sĩ xác minh trước khi thanh toán thuốc kê đơn.
        </p>
      </div>
    )
  }

  return (
    <div className="space-y-3">
      {prescriptions.map((p) => {
        const selected = p.id === selectedId
        return (
          <label
            key={p.id}
            onClick={() => onSelect(p.id)}
            className={`relative flex items-start gap-4 p-5 border-2 rounded-xl cursor-pointer transition-all ${
              selected
                ? 'bg-surface-container-lowest border-primary shadow-sm'
                : 'bg-surface-container-low border-transparent hover:border-outline-variant'
            }`}
          >
            <input type="radio" className="hidden" checked={selected} readOnly />
            <div className="w-11 h-11 rounded-lg bg-primary-fixed flex items-center justify-center shrink-0 text-on-primary-fixed">
              <span className="material-symbols-outlined">clinical_notes</span>
            </div>
            <div className="flex-1 min-w-0">
              <div className="font-bold">
                {p.title || `Đơn thuốc #${p.id}`}
              </div>
              <div className="text-sm text-on-surface-variant mt-0.5">
                Bác sĩ: {p.doctorNameSnapshot || 'Không rõ'} • {p.itemCount} thuốc
              </div>
              <div className="mt-2 text-xs inline-flex px-2 py-1 rounded-full bg-primary-container/60 text-primary font-semibold">
                {statusLabel(p.verificationStatus)}
              </div>
            </div>
            {selected && (
              <span className="material-symbols-outlined text-primary" style={{ fontVariationSettings: "'FILL' 1" }}>
                check_circle
              </span>
            )}
          </label>
        )
      })}
    </div>
  )
}
