// =============================================================================
// Medications API - public list/get + admin CRUD (POST/PUT/DELETE).
// Admin endpoints yeu cau JWT cua user role=admin (BE gate bang Authorize).
// =============================================================================
import { httpClient } from '@/services/http-client'
import type { Medication, MedicationListQuery, MedicationsPaged } from './types'

export type MedicationUpsertRequest = {
  sku: string
  name: string
  genericName: string | null
  manufacturer: string | null
  registrationNumber: string | null
  description: string | null
  dosage: string | null
  packaging: string | null
  price: number
  discountPercent: number
  categoryId: number
  usageInstructions: string | null
  benefits: string | null
  activeIngredients: string | null
  contraindications: string | null
  sideEffects: string | null
  storageInstructions: string | null
  imageUrl: string | null
  isFeatured: boolean
  isBestSeller: boolean
  isPrescriptionRequired: boolean
  stockQuantity: number
  isActive: boolean
}

export const medicationsApi = {
  list: async (query: MedicationListQuery = {}): Promise<MedicationsPaged> => {
    const res = await httpClient.get<MedicationsPaged>('/medications', { params: query })
    return res.data
  },
  getById: async (id: number): Promise<Medication> => {
    const res = await httpClient.get<Medication>(`/medications/${id}`)
    return res.data
  },
  create: async (body: MedicationUpsertRequest): Promise<Medication> => {
    const res = await httpClient.post<Medication>('/medications', body)
    return res.data
  },
  update: async (id: number, body: MedicationUpsertRequest): Promise<Medication> => {
    const res = await httpClient.put<Medication>(`/medications/${id}`, body)
    return res.data
  },
  delete: async (id: number): Promise<void> => {
    await httpClient.delete(`/medications/${id}`)
  },
  /** Upload ảnh sản phẩm lên Cloudinary qua backend. Trả về URL ảnh. */
  uploadImage: async (file: File): Promise<string> => {
    const form = new FormData()
    form.append('file', file)
    const res = await httpClient.post<{ url: string }>('/admin/images/upload', form, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })
    return res.data.url
  },
}

