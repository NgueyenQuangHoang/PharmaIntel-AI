import { httpClient } from '@/services/http-client'
import type { Paged } from '@/features/categories/types'
import type {
  Prescription,
  PrescriptionCreateRequest,
  PrescriptionDocument,
  PrescriptionItem,
  PrescriptionItemCreateRequest,
  PrescriptionListItem,
  PrescriptionUpdateRequest,
} from './types'

export type PrescriptionListQuery = {
  page?: number
  pageSize?: number
  status?: string
  q?: string
}

export const prescriptionsApi = {
  listMy: async (query: PrescriptionListQuery = {}) => {
    const res = await httpClient.get<Paged<PrescriptionListItem>>('/prescriptions/my', {
      params: { page: 1, pageSize: 20, ...query },
    })
    return res.data
  },

  getById: async (id: number) => {
    const res = await httpClient.get<Prescription>(`/prescriptions/${id}`)
    return res.data
  },

  create: async (body: PrescriptionCreateRequest) => {
    const res = await httpClient.post<Prescription>('/prescriptions', body)
    return res.data
  },

  update: async (id: number, body: PrescriptionUpdateRequest) => {
    const res = await httpClient.put<Prescription>(`/prescriptions/${id}`, body)
    return res.data
  },

  addItem: async (id: number, body: PrescriptionItemCreateRequest) => {
    const res = await httpClient.post<PrescriptionItem>(`/prescriptions/${id}/items`, body)
    return res.data
  },

  listDocuments: async (id: number) => {
    const res = await httpClient.get<PrescriptionDocument[]>(`/prescriptions/${id}/documents`)
    return res.data
  },

  uploadDocument: async (id: number, file: File) => {
    const form = new FormData()
    form.append('file', file)

    const res = await httpClient.post<PrescriptionDocument>(
      `/prescriptions/${id}/documents`,
      form,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      },
    )

    return res.data
  },
}
