import { useState } from 'react'
import axios from 'axios'
import { prescriptionsApi } from '../prescriptions-api'
import type { PrescriptionDocument } from '../types'

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

export function PrescriptionDocumentUploader({
  prescriptionId,
  onUploaded,
}: {
  prescriptionId: number
  onUploaded: (doc: PrescriptionDocument) => void
}) {
  const [file, setFile] = useState<File | null>(null)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function upload() {
    if (!file) {
      setError('Vui lòng chọn file đơn thuốc')
      return
    }

    setBusy(true)
    setError(null)

    try {
      const doc = await prescriptionsApi.uploadDocument(prescriptionId, file)
      onUploaded(doc)
      setFile(null)
    } catch (err) {
      setError(extractApiError(err, 'Upload đơn thuốc thất bại'))
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="p-5 rounded-xl bg-surface-container-low border border-outline-variant/20">
      <div className="font-bold mb-2">Upload ảnh/PDF đơn thuốc</div>
      <p className="text-sm text-on-surface-variant mb-4">
        Hỗ trợ JPG, PNG, WEBP hoặc PDF. Sau khi upload, dược sĩ sẽ xác minh.
      </p>

      <input
        type="file"
        accept=".jpg,.jpeg,.png,.webp,.pdf,image/jpeg,image/png,image/webp,application/pdf"
        onChange={(e) => setFile(e.target.files?.[0] ?? null)}
        className="block w-full text-sm mb-4 file:mr-4 file:py-2.5 file:px-5 file:rounded-xl file:border-0 file:text-sm file:font-bold file:bg-primary/10 file:text-primary hover:file:bg-primary/20 file:cursor-pointer text-on-surface-variant cursor-pointer transition-colors"
      />

      {error && (
        <div className="mb-3 p-3 rounded-lg bg-error-container text-on-error-container text-sm">
          {error}
        </div>
      )}

      <button
        type="button"
        disabled={!file || busy}
        onClick={upload}
        className="px-5 py-2.5 rounded-xl bg-primary text-on-primary font-semibold disabled:opacity-50"
      >
        {busy ? 'Đang upload...' : 'Gửi đơn thuốc'}
      </button>
    </div>
  )
}
