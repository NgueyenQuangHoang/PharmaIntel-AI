// =============================================================================
// Admin API - GET/PUT/DELETE /api/admin/*
// Yeu cau JWT cua user co role=admin (httpClient tu attach Bearer).
// =============================================================================
import { httpClient } from '@/services/http-client'
import type {
  AdminOrdersPaged,
  AdminStatsOverview,
  AdminUser,
  AdminUserListQuery,
  AdminUsersPaged,
  OrdersByStatus,
  RevenuePoint,
  RevenueRange,
  TopMedication,
  UpdateUserRoleRequest,
  UpdateUserStatusRequest,
} from './types'

export type AdminOrderListQuery = {
  page?: number
  pageSize?: number
  status?: string
  paymentStatus?: string
}

export const adminApi = {
  users: {
    list: async (query: AdminUserListQuery = {}): Promise<AdminUsersPaged> => {
      const params: Record<string, unknown> = { ...query }
      // axios serialize null/undefined la "null" -> bo de BE bo qua
      if (params.isActive === null) delete params.isActive
      if (params.role === '') delete params.role
      const res = await httpClient.get<AdminUsersPaged>('/admin/users', { params })
      return res.data
    },
    get: async (id: number): Promise<AdminUser> => {
      const res = await httpClient.get<AdminUser>(`/admin/users/${id}`)
      return res.data
    },
    updateRole: async (id: number, body: UpdateUserRoleRequest): Promise<AdminUser> => {
      const res = await httpClient.put<AdminUser>(`/admin/users/${id}/role`, body)
      return res.data
    },
    updateStatus: async (id: number, body: UpdateUserStatusRequest): Promise<AdminUser> => {
      const res = await httpClient.put<AdminUser>(`/admin/users/${id}/status`, body)
      return res.data
    },
    delete: async (id: number): Promise<void> => {
      await httpClient.delete(`/admin/users/${id}`)
    },
  },
  orders: {
    list: async (query: AdminOrderListQuery = {}): Promise<AdminOrdersPaged> => {
      const params: Record<string, unknown> = { ...query }
      if (params.status === '' || params.status === undefined) delete params.status
      if (params.paymentStatus === '' || params.paymentStatus === undefined) delete params.paymentStatus
      const res = await httpClient.get<AdminOrdersPaged>('/orders/admin/all', { params })
      return res.data
    },
    updateStatus: async (id: number, status: string): Promise<unknown> => {
      const res = await httpClient.put(`/orders/${id}/status`, { status })
      return res.data
    },
  },
  stats: {
    overview: async (): Promise<AdminStatsOverview> => {
      const res = await httpClient.get<AdminStatsOverview>('/admin/stats/overview')
      return res.data
    },
    revenue: async (range: RevenueRange = '7d'): Promise<RevenuePoint[]> => {
      const res = await httpClient.get<RevenuePoint[]>('/admin/stats/revenue', { params: { range } })
      return res.data
    },
    topMedications: async (limit = 10): Promise<TopMedication[]> => {
      const res = await httpClient.get<TopMedication[]>('/admin/stats/top-medications', { params: { limit } })
      return res.data
    },
    ordersByStatus: async (): Promise<OrdersByStatus[]> => {
      const res = await httpClient.get<OrdersByStatus[]>('/admin/stats/orders-by-status')
      return res.data
    },
  },
}
