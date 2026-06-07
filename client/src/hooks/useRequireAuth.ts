// =============================================================================
// useRequireAuth - gate hanh dong can dang nhap.
// Tra ve ham requireAuth(): neu da dang nhap -> true; neu chua -> dieu huong toi
// /login?redirect=<trang hien tai> va tra false. Dung cho cac nut hanh dong tren
// trang cong khai (them gio hang, dat lich, chat...) khi khach chua dang nhap.
// =============================================================================
import { useCallback } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '@/hooks/useAuth'

export function useRequireAuth() {
  const { isAuthenticated } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()

  return useCallback(() => {
    if (isAuthenticated) return true
    const redirect = encodeURIComponent(location.pathname + location.search)
    navigate(`/login?redirect=${redirect}`)
    return false
  }, [isAuthenticated, navigate, location])
}
