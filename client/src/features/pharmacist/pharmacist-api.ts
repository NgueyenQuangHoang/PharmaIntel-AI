import { httpClient } from '@/services/http-client'
import type { Paged } from '@/features/categories/types'
import type {
  Prescription,
  PrescriptionItem,
  PrescriptionItemCreateRequest,
} from '@/features/prescriptions/types'
import type {
  PendingPrescriptionDocumentQuery,
  PrescriptionDocumentDecisionRequest,
  PrescriptionDocumentHistoryQuery,
  PrescriptionDocumentVerification,
} from './types'

export const pharmacistApi = {
  listPendingDocuments: async (query: PendingPrescriptionDocumentQuery = {}) => {
    const res = await httpClient.get<Paged<PrescriptionDocumentVerification>>(
      '/pharmacist/prescription-documents/pending',
      {
        params: { page: 1, pageSize: 20, ...query },
      },
    )
    return res.data
  },

  listHistoryDocuments: async (query: PrescriptionDocumentHistoryQuery = {}) => {
    const res = await httpClient.get<Paged<PrescriptionDocumentVerification>>(
      '/pharmacist/prescription-documents/history',
      {
        params: { page: 1, pageSize: 20, ...query },
      },
    )
    return res.data
  },

  verifyDocument: async (id: number, body: PrescriptionDocumentDecisionRequest) => {
    const res = await httpClient.put<PrescriptionDocumentVerification>(
      `/pharmacist/prescription-documents/${id}/verify`,
      body,
    )
    return res.data
  },

  rejectDocument: async (id: number, body: PrescriptionDocumentDecisionRequest) => {
    const res = await httpClient.put<PrescriptionDocumentVerification>(
      `/pharmacist/prescription-documents/${id}/reject`,
      body,
    )
    return res.data
  },

  getPrescription: async (id: number) => {
    const res = await httpClient.get<Prescription>(`/pharmacist/prescriptions/${id}`)
    return res.data
  },

  addPrescriptionItem: async (prescriptionId: number, body: PrescriptionItemCreateRequest) => {
    const res = await httpClient.post<PrescriptionItem>(
      `/pharmacist/prescriptions/${prescriptionId}/items`,
      body,
    )
    return res.data
  },

  updatePrescriptionItem: async (itemId: number, body: PrescriptionItemCreateRequest) => {
    const res = await httpClient.put<PrescriptionItem>(
      `/pharmacist/prescription-items/${itemId}`,
      body,
    )
    return res.data
  },

  removePrescriptionItem: async (itemId: number) => {
    await httpClient.delete(`/pharmacist/prescription-items/${itemId}`)
  },
}
