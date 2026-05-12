// =============================================================================
// useAuth - hook tien dung doc state.auth + dispatch login/register/logout
// =============================================================================
import { useCallback } from 'react'
import { useAppDispatch, useAppSelector } from '@/hooks/redux'
import {
  fetchMeThunk,
  loginThunk,
  logoutThunk,
  registerThunk,
} from '@/features/auth/auth-slice'
import type { LoginRequest, RegisterRequest } from '@/features/auth/types'

export function useAuth() {
  const dispatch = useAppDispatch()
  const { token, user, status, error } = useAppSelector((s) => s.auth)

  const login = useCallback(
    (body: LoginRequest) => dispatch(loginThunk(body)).unwrap(),
    [dispatch],
  )
  const register = useCallback(
    (body: RegisterRequest) => dispatch(registerThunk(body)).unwrap(),
    [dispatch],
  )
  const fetchMe = useCallback(() => dispatch(fetchMeThunk()).unwrap(), [dispatch])
  const logout = useCallback(() => dispatch(logoutThunk()).unwrap(), [dispatch])

  return {
    token,
    user,
    status,
    error,
    isAuthenticated: status === 'authenticated' && !!token,
    login,
    register,
    fetchMe,
    logout,
  }
}
