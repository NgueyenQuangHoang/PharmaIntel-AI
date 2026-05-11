// =============================================================================
// resolveFileUrl - ghep relative file URL tu BE (vd "/uploads/...") thanh URL
// day du de browser load. Backend tra ve fileUrl la relative path; FE phai cong
// voi API origin (BE host) chu khong phai VITE_API_URL (co them /api).
//
// Vi du:
//   VITE_API_URL = http://localhost:5292/api
//   fileUrl       = /uploads/prescriptions/1/2026-05-10/abc.jpg
//   ket qua       = http://localhost:5292/uploads/prescriptions/1/2026-05-10/abc.jpg
//
// Note: Don thuoc la du lieu y te - prod nen chuyen sang endpoint co auth.
// =============================================================================

const apiBase = import.meta.env.VITE_API_URL ?? 'http://localhost:5292/api'
const apiOrigin = apiBase.replace(/\/api\/?$/, '')

export function resolveFileUrl(fileUrl: string | null | undefined): string {
  if (!fileUrl) return ''
  if (/^https?:\/\//i.test(fileUrl)) return fileUrl
  return `${apiOrigin}${fileUrl.startsWith('/') ? fileUrl : `/${fileUrl}`}`
}
