import { httpClient } from '@/services/http-client'
import type { Paged } from '@/features/categories/types'
import type {
  PendingPrescriptionDocumentQuery,
  PrescriptionDocumentDecisionRequest,
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
}
