// =============================================================================
// Categories API - public list + admin CRUD (POST/PUT/DELETE).
// Admin endpoints yeu cau JWT cua user role=admin (BE gate bang Authorize).
// =============================================================================
import { httpClient } from '@/services/http-client'
import type { Category, CategoryListQuery, Paged } from './types'

export type CategoryUpsertRequest = {
  name: string
  slug?: string
  icon: string | null
  displayOrder: number
  isActive: boolean
}

export const categoriesApi = {
  list: async (query: CategoryListQuery = {}): Promise<Paged<Category>> => {
    const res = await httpClient.get<Paged<Category>>('/categories', { params: query })
    return res.data
  },
  create: async (body: CategoryUpsertRequest): Promise<Category> => {
    const res = await httpClient.post<Category>('/categories', body)
    return res.data
  },
  update: async (id: number, body: CategoryUpsertRequest): Promise<Category> => {
    const res = await httpClient.put<Category>(`/categories/${id}`, body)
    return res.data
  },
  delete: async (id: number): Promise<void> => {
    await httpClient.delete(`/categories/${id}`)
  },
}
