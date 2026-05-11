import { useEffect, useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { prescriptionsApi } from '@/features/prescriptions/prescriptions-api'
import type { Prescription, PrescriptionDocument } from '@/features/prescriptions/types'
import { PrescriptionDocumentUploader } from '@/features/prescriptions/components/PrescriptionDocumentUploader'
import { resolveFileUrl } from '@/utils/file-url'

export function PrescriptionDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const prescriptionId = Number(id)

  const [prescription, setPrescription] = useState<Prescription | null>(null)
  const [documents, setDocuments] = useState<PrescriptionDocument[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!prescriptionId) return

    let cancelled = false
    setLoading(true)

    Promise.all([
      prescriptionsApi.getById(prescriptionId),
      prescriptionsApi.listDocuments(prescriptionId),
    ])
      .then(([p, docs]) => {
        if (!cancelled) {
          setPrescription(p)
          setDocuments(docs)
        }
      })
      .catch(() => {
        if (!cancelled) setError('Không tải được chi tiết đơn thuốc')
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })

    return () => {
      cancelled = true
    }
  }, [prescriptionId])

  if (loading) {
    return <div className="p-8 text-center mt-10">Đang tải chi tiết...</div>
  }

  if (error || !prescription) {
    return <div className="p-8 text-center text-error mt-10">{error || 'Không tìm thấy đơn thuốc'}</div>
  }

  return (
    <div className="pt-8 pb-24 px-6 md:px-8 max-w-4xl mx-auto animate-in fade-in zoom-in-95 duration-500">
      <button
        onClick={() => navigate(-1)}
        className="mb-6 flex items-center gap-1 text-sm font-semibold text-on-surface-variant hover:text-primary transition-colors"
      >
        <span className="material-symbols-outlined text-[18px]">arrow_back</span>
        Quay lại
      </button>

      <header className="mb-8">
        <h1 className="text-3xl font-extrabold tracking-tight mb-2">
          {prescription.title || `Đơn thuốc #${prescription.id}`}
        </h1>
        <div className="flex flex-wrap gap-2 mb-4">
          <span className="px-3 py-1 rounded-full text-xs font-semibold bg-surface-container-high">
            Trạng thái: {prescription.status}
          </span>
          <span className="px-3 py-1 rounded-full text-xs font-semibold bg-primary-container text-on-primary-container">
            Xác minh: {prescription.verificationStatus}
          </span>
        </div>
      </header>

      <div className="grid gap-6 md:grid-cols-2">
        <section className="p-5 rounded-2xl bg-surface-container-lowest border border-outline-variant/20 shadow-sm">
          <h2 className="font-bold text-lg mb-4 flex items-center gap-2">
            <span className="material-symbols-outlined text-primary">person</span>
            Thông tin đơn thuốc
          </h2>
          <div className="space-y-3 text-sm">
            <div className="flex justify-between">
              <span className="text-on-surface-variant">Bác sĩ:</span>
              <span className="font-medium">{prescription.doctorNameSnapshot || 'Không rõ'}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-on-surface-variant">Ngày kê:</span>
              <span className="font-medium">
                {prescription.prescribedDate ? new Date(prescription.prescribedDate).toLocaleDateString('vi-VN') : 'Không rõ'}
              </span>
            </div>
            <div className="flex justify-between">
              <span className="text-on-surface-variant">Tạo lúc:</span>
              <span className="font-medium">{new Date(prescription.createdAt).toLocaleString('vi-VN')}</span>
            </div>
          </div>
        </section>

        <section className="p-5 rounded-2xl bg-surface-container-lowest border border-outline-variant/20 shadow-sm">
          <h2 className="font-bold text-lg mb-4 flex items-center gap-2">
            <span className="material-symbols-outlined text-primary">medication</span>
            Danh sách thuốc
          </h2>
          {prescription.items.length === 0 ? (
            <p className="text-sm text-on-surface-variant">Không có thuốc nào trong đơn.</p>
          ) : (
            <ul className="space-y-3">
              {prescription.items.map((item) => (
                <li key={item.id} className="text-sm border-b border-outline-variant/10 pb-2 last:border-0">
                  <div className="font-bold">{item.medicationName}</div>
                  <div className="text-on-surface-variant text-xs mt-1">
                    {item.dosage} • {item.frequency} • {item.duration}
                  </div>
                </li>
              ))}
            </ul>
          )}
        </section>

        <section className="md:col-span-2 p-5 rounded-2xl bg-surface-container-lowest border border-outline-variant/20 shadow-sm">
          <h2 className="font-bold text-lg mb-4 flex items-center gap-2">
            <span className="material-symbols-outlined text-primary">attach_file</span>
            Tài liệu đính kèm
          </h2>
          
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3 mb-6">
            {documents.map((doc) => (
              <div key={doc.id} className="p-3 rounded-xl border border-outline-variant/20 bg-surface-container-low flex flex-col gap-2">
                <a
                  href={resolveFileUrl(doc.fileUrl)}
                  target="_blank"
                  rel="noreferrer"
                  className="font-semibold text-primary text-sm break-all hover:underline"
                >
                  Tài liệu #{doc.id}
                </a>
                <div className="text-xs">
                  Trạng thái:{' '}
                  <span className={
                    doc.verificationStatus === 'verified' ? 'text-green-600' :
                    doc.verificationStatus === 'rejected' ? 'text-red-600' : 'text-yellow-600'
                  }>
                    {doc.verificationStatus}
                  </span>
                </div>
                {doc.notes && (
                  <div className="text-xs text-on-surface-variant bg-surface p-2 rounded-lg mt-auto">
                    Ghi chú: {doc.notes}
                  </div>
                )}
              </div>
            ))}
            {documents.length === 0 && (
              <div className="text-sm text-on-surface-variant col-span-full">Chưa có tài liệu nào.</div>
            )}
          </div>

          <PrescriptionDocumentUploader
            prescriptionId={prescriptionId}
            onUploaded={(doc) => setDocuments((prev) => [...prev, doc])}
          />
        </section>
      </div>
    </div>
  )
}
