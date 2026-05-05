// =============================================================================
// Cart API - tat ca yeu cau JWT
// =============================================================================
import { httpClient } from '@/services/http-client'
import type { Cart } from './types'

export const cartApi = {
  get: async (): Promise<Cart> => {
    const res = await httpClient.get<Cart>('/cart')
    return res.data
  },
  addItem: async (medicationId: number, quantity = 1): Promise<Cart> => {
    const res = await httpClient.post<Cart>('/cart/items', { medicationId, quantity })
    return res.data
  },
  updateItem: async (medicationId: number, quantity: number): Promise<Cart> => {
    const res = await httpClient.put<Cart>(`/cart/items/${medicationId}`, { quantity })
    return res.data
  },
  removeItem: async (medicationId: number): Promise<Cart> => {
    const res = await httpClient.delete<Cart>(`/cart/items/${medicationId}`)
    return res.data
  },
  clear: async (): Promise<void> => {
    await httpClient.delete('/cart')
  },
}
