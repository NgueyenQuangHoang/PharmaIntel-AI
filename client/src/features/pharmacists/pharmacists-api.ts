// =============================================================================
// Pharmacists API - public list/get + admin CRUD.
// Endpoint chinh:
//   GET    /pharmacists       cong khai (trang Tu van)
//   GET    /pharmacists/:id   cong khai
//   POST   /pharmacists       admin only
//   PUT    /pharmacists/:id   admin only
//   DELETE /pharmacists/:id   admin only
// =============================================================================
import { httpClient } from '@/services/http-client'
import type {
  Pharmacist,
  PharmacistListQuery,
  PharmacistsPaged,
} from './types'

export type PharmacistUpsertRequest = {
  fullName: string
  licenseNumber?: string | null
  specialization: string | null
  phone: string | null
  email: string | null
  avatarUrl: string | null
  isOnline: boolean
  isActive: boolean
  experienceYears: number
  about: string | null
  rating: number
  reviewsCount: number
}

export const pharmacistsApi = {
  list: async (query: PharmacistListQuery = {}): Promise<PharmacistsPaged> => {
    const res = await httpClient.get<PharmacistsPaged>('/pharmacists', { params: query })
    return res.data
  },
  getById: async (id: number): Promise<Pharmacist> => {
    const res = await httpClient.get<Pharmacist>(`/pharmacists/${id}`)
    return res.data
  },
  create: async (body: PharmacistUpsertRequest): Promise<Pharmacist> => {
    const res = await httpClient.post<Pharmacist>('/pharmacists', body)
    return res.data
  },
  update: async (id: number, body: PharmacistUpsertRequest): Promise<Pharmacist> => {
    const res = await httpClient.put<Pharmacist>(`/pharmacists/${id}`, body)
    return res.data
  },
  delete: async (id: number): Promise<void> => {
    await httpClient.delete(`/pharmacists/${id}`)
  },
}
