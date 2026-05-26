// =============================================================================
// Consultations API - dat lich tu van + dashboard duoc si.
// Endpoint:
//   POST /consultations              user dat lich
//   GET  /consultations/my           lich su cua user
//   GET  /pharmacist/consultations   duoc si xem hang cho
//   PUT  /pharmacist/consultations/:id/status   duoc si duyet/tu choi
// =============================================================================
import { httpClient } from '@/services/http-client'
import type {
  Consultation,
  ConsultationsListQuery,
  ConsultationsPaged,
  CreateConsultationRequest,
  UpdateConsultationStatusRequest,
} from './types'

export const consultationsApi = {
  create: async (body: CreateConsultationRequest): Promise<Consultation> => {
    const res = await httpClient.post<Consultation>('/consultations', body)
    return res.data
  },
  listMine: async (query: ConsultationsListQuery = {}): Promise<ConsultationsPaged> => {
    const res = await httpClient.get<ConsultationsPaged>('/consultations/my', { params: query })
    return res.data
  },
  listForPharmacist: async (query: ConsultationsListQuery = {}): Promise<ConsultationsPaged> => {
    const res = await httpClient.get<ConsultationsPaged>('/pharmacist/consultations', { params: query })
    return res.data
  },
  updateStatus: async (id: number, body: UpdateConsultationStatusRequest): Promise<Consultation> => {
    const res = await httpClient.put<Consultation>(`/pharmacist/consultations/${id}/status`, body)
    return res.data
  },
}
