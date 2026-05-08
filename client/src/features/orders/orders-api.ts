// =============================================================================
// Orders API - tat ca yeu cau JWT
// =============================================================================
import { httpClient } from '@/services/http-client'
import type { PagedResult } from '@/features/addresses/types'
import type { CheckoutRequest, OrderDto, OrderListItemDto } from './types'

export type OrdersListQuery = {
  page?: number
  pageSize?: number
  status?: string
  paymentStatus?: string
}

export const ordersApi = {
  checkout: async (req: CheckoutRequest): Promise<OrderDto> => {
    const res = await httpClient.post<OrderDto>('/orders/checkout', req)
    return res.data
  },
  getById: async (id: number): Promise<OrderDto> => {
    const res = await httpClient.get<OrderDto>(`/orders/${id}`)
    return res.data
  },
  listMy: async (query: OrdersListQuery = {}): Promise<PagedResult<OrderListItemDto>> => {
    const res = await httpClient.get<PagedResult<OrderListItemDto>>('/orders/my', {
      params: { page: 1, pageSize: 20, ...query },
    })
    return res.data
  },
}
