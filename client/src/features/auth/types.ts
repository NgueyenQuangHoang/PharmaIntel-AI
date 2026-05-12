// =============================================================================
// Auth types - khop voi DTO backend (server/PharmaIntel.Core/DTOs/Auth)
// JSON tu ASP.NET Core mac dinh la camelCase nen field cung camelCase.
// =============================================================================

export type LoginRequest = {
  email: string
  password: string
}

export type RegisterRequest = {
  fullName: string
  email: string
  password: string
  isTermsAccepted: boolean
}

export type UserInfo = {
  id: number
  fullName: string
  email: string
  avatarUrl: string | null
  authProvider: string
  role: string
  isActive: boolean
  createdAt: string
}

export type AuthResponse = {
  accessToken: string
  tokenType: string
  expiresIn: number
  refreshToken: string
  refreshTokenExpiresAt: string
  user: UserInfo
}

export type RefreshRequest = {
  refreshToken: string
}

export type LogoutRequest = {
  refreshToken: string
}
