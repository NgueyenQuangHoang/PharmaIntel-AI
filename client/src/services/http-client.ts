// =============================================================================
// HTTP client - axios voi interceptors:
//  - Request: dinh kem Authorization Bearer neu co token
//  - Response: bat 401 -> thu /auth/refresh truoc; neu refresh OK -> retry request goc;
//              neu fail -> clear token + dispatch logout + redirect /login.
//              Dedupe: nhieu request song song dung 1 inflight refresh promise.
// =============================================================================
import axios, {
  type AxiosRequestConfig,
  type InternalAxiosRequestConfig,
} from 'axios'
import {
  clearRefreshToken,
  clearToken,
  getRefreshToken,
  getToken,
  setRefreshToken,
  setToken,
} from '@/features/auth/token-storage'

const baseURL = import.meta.env.VITE_API_URL ?? 'http://localhost:5292/api'

export const httpClient = axios.create({
  baseURL,
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

// Dedupe: nhieu request 401 song song chi trigger 1 refresh call, cac request con lai await chung promise.
let refreshPromise: Promise<string> | null = null

async function refreshAccessToken(): Promise<string> {
  if (refreshPromise) return refreshPromise
  refreshPromise = (async () => {
    const refresh = getRefreshToken()
    if (!refresh) throw new Error('no_refresh_token')
    // Goi axios truc tiep (khong qua httpClient) de tranh interceptor lap voi 401.
    const res = await axios.post<{
      accessToken: string
      refreshToken: string
    }>(`${baseURL}/auth/refresh`, { refreshToken: refresh }, {
      headers: { 'Content-Type': 'application/json' },
      timeout: 10_000,
    })
    setToken(res.data.accessToken)
    setRefreshToken(res.data.refreshToken)
    return res.data.accessToken
  })().finally(() => {
    refreshPromise = null
  })
  return refreshPromise
}

async function handleAuthFailureFinal(): Promise<void> {
  clearToken()
  clearRefreshToken()
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

type RetriableConfig = AxiosRequestConfig & { _retry?: boolean }

httpClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const status = error?.response?.status
    const original = (error?.config ?? {}) as RetriableConfig
    const url: string = original.url ?? ''
    const isAuthEndpoint =
      url.includes('/auth/login') ||
      url.includes('/auth/register') ||
      url.includes('/auth/refresh') ||
      url.includes('/auth/logout')

    if (status !== 401 || isAuthEndpoint || original._retry) {
      // 401 tu /auth/refresh hoac retry da fail -> clear va redirect.
      if (status === 401 && (url.includes('/auth/refresh') || original._retry)) {
        await handleAuthFailureFinal()
      }
      return Promise.reject(error)
    }

    original._retry = true
    try {
      const newToken = await refreshAccessToken()
      original.headers = { ...(original.headers ?? {}), Authorization: `Bearer ${newToken}` }
      return await httpClient(original)
    } catch {
      await handleAuthFailureFinal()
      return Promise.reject(error)
    }
  },
)
