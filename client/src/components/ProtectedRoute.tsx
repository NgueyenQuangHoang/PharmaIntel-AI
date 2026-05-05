// =============================================================================
// ProtectedRoute - chan route khi chua co token, hien spinner khi dang hydrate.
// Tuy chon requireRole de gate theo role (vd "admin"); user khong du quyen
// duoc redirect ve trang chu thay vi /login (token van hop le).
// =============================================================================
import type { ReactNode } from 'react'
import { Navigate } from 'react-router-dom'
import { useAppSelector } from '@/hooks/redux'

type Props = {
  children: ReactNode
  requireRole?: string
}

export function ProtectedRoute({ children, requireRole }: Props) {
  const { token, status, user } = useAppSelector((s) => s.auth)

  if (!token) {
    return <Navigate to="/login" replace />
  }

  if (status === 'loading' && !user) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-surface">
        <div className="flex flex-col items-center gap-3">
          <span className="material-symbols-outlined text-primary text-4xl animate-spin">progress_activity</span>
          <span className="font-body text-sm text-on-surface-variant">Dang xac thuc...</span>
        </div>
      </div>
    )
  }

  if (requireRole && user && user.role?.toLowerCase() !== requireRole.toLowerCase()) {
    return <Navigate to="/" replace />
  }

  return <>{children}</>
}
