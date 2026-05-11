// =============================================================================
// Admin types - khop voi BE (server/PharmaIntel.Core/DTOs/Admin)
// =============================================================================
import type { Paged } from '@/features/categories/types'

export type AppRole = 'user' | 'admin' | 'pharmacist'

export type AdminUser = {
  id: number
  fullName: string
  email: string
  avatarUrl: string | null
  role: AppRole | string
  isActive: boolean
  authProvider: string
  totalOrders: number
  totalSpent: number
  createdAt: string
  updatedAt: string
}

export type AdminUserListQuery = {
  page?: number
  pageSize?: number
  q?: string
  role?: AppRole | ''
  isActive?: boolean | null
}

export type AdminUsersPaged = Paged<AdminUser>

export type UpdateUserRoleRequest = { role: AppRole }
export type UpdateUserStatusRequest = { isActive: boolean }

// === Stats ===

export type AdminStatsOverview = {
  totalUsers: number
  totalAdmins: number
  activeUsers: number
  totalOrders: number
  totalRevenue: number
  totalMedications: number
  totalCategories: number
  ordersToday: number
  revenueToday: number
  ordersPending: number
}

export type RevenuePoint = {
  date: string // ISO date "YYYY-MM-DD"
  revenue: number
  orderCount: number
}

export type TopMedication = {
  medicationId: number
  name: string
  imageUrl: string | null
  quantitySold: number
  revenue: number
}

export type OrdersByStatus = {
  status: string
  count: number
}

export type RevenueRange = '7d' | '30d' | '90d'

// === Orders (admin view) - minimal cho trang Orders ===

export type AdminOrderItem = {
  id: number
  orderCode: string
  userId: number
  userFullName?: string | null
  userEmail?: string | null
  total: number
  status: string
  paymentStatus: string
  createdAt: string
}

export type AdminOrdersPaged = Paged<AdminOrderItem>
