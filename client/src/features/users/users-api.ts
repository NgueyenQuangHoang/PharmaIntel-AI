// =============================================================================
// Users API - profile cua user dang dang nhap (JWT bearer).
// Endpoints:
//   GET   /users/me                       lay profile
//   PUT   /users/me                       cap nhat profile (FullName, AvatarUrl, PhoneNumber, DateOfBirth)
//   PUT   /users/me/change-password       doi mat khau (yeu cau CurrentPassword)
//   POST  /users/me/images/upload         upload avatar len Cloudinary (multipart field "file")
// =============================================================================
import { httpClient } from '@/services/http-client'

export type UserProfile = {
  id: number
  fullName: string
  email: string
  avatarUrl: string | null
  phoneNumber: string | null
  dateOfBirth: string | null  // ISO yyyy-MM-dd
  authProvider: string
  role: string
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export type UpdateProfileRequest = {
  fullName: string
  avatarUrl: string | null
  phoneNumber: string | null
  dateOfBirth: string | null
}

export type ChangePasswordRequest = {
  currentPassword: string
  newPassword: string
  confirmPassword: string
}

export const usersApi = {
  getMe: async (): Promise<UserProfile> => {
    const res = await httpClient.get<UserProfile>('/users/me')
    return res.data
  },
  updateProfile: async (body: UpdateProfileRequest): Promise<UserProfile> => {
    const res = await httpClient.put<UserProfile>('/users/me', body)
    return res.data
  },
  changePassword: async (body: ChangePasswordRequest): Promise<void> => {
    await httpClient.put('/users/me/change-password', body)
  },
  uploadAvatar: async (file: File): Promise<string> => {
    const form = new FormData()
    form.append('file', file)
    const res = await httpClient.post<{ url: string }>('/users/me/images/upload', form, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })
    return res.data.url
  },
}
