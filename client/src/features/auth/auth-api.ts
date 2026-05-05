// =============================================================================
// Auth API - wrapper goi /api/auth/* dung lai httpClient
// =============================================================================
import { httpClient } from '@/services/http-client'
import type {
  AuthResponse,
  LoginRequest,
  RegisterRequest,
  UserInfo,
} from './types'

export const authApi = {
  login: async (body: LoginRequest): Promise<AuthResponse> => {
    const res = await httpClient.post<AuthResponse>('/auth/login', body)
    return res.data
  },
  register: async (body: RegisterRequest): Promise<AuthResponse> => {
    const res = await httpClient.post<AuthResponse>('/auth/register', body)
    return res.data
  },
  me: async (): Promise<UserInfo> => {
    const res = await httpClient.get<UserInfo>('/auth/me')
    return res.data
  },
}
