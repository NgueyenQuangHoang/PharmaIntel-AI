// =============================================================================
// HTTP client - axios voi interceptors:
//  - Request: dinh kem Authorization Bearer neu co token trong localStorage
//  - Response: bat 401 -> clear token + dispatch logout + redirect /login
// =============================================================================
import axios, { type InternalAxiosRequestConfig } from 'axios'
import { clearToken, getToken } from '@/features/auth/token-storage'

export const httpClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5292/api',
  timeout: 10_000,
  headers: {
    'Content-Type': 'application/json',
  },
})

httpClient.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const token = getToken()
  if (token) {
    config.headers.set('Authorization', `Bearer ${token}`)
  }
  return config
})

httpClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const status = error?.response?.status
    const url: string = error?.config?.url ?? ''
    const isAuthEndpoint = url.includes('/auth/login') || url.includes('/auth/register')

    if (status === 401 && !isAuthEndpoint) {
      clearToken()
      // Lazy import de tranh circular: store -> auth-slice -> http-client -> store
      try {
        const { store } = await import('@/app/store')
        const { logout } = await import('@/features/auth/auth-slice')
        store.dispatch(logout())
      } catch {
        /* ignore */
      }
      if (typeof window !== 'undefined' && window.location.pathname !== '/login') {
        window.location.assign('/login')
      }
    }

    return Promise.reject(error)
  },
)
