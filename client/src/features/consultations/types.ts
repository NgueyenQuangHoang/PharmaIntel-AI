// =============================================================================
// Consultation types - khop voi PharmaIntel.Core/DTOs/Consultations
// =============================================================================
import type { Paged } from '@/features/categories/types'

export type ConsultationStatus = 'pending' | 'accepted' | 'rejected' | 'completed' | 'cancelled'

export type Consultation = {
  id: number
  userId: number
  userFullName: string
  userEmail: string | null
  userPhoneNumber: string | null
  pharmacistId: number
  pharmacistName: string
  scheduledAt: string
  note: string | null
  status: ConsultationStatus
  responseNote: string | null
  createdAt: string
  updatedAt: string
}

export type CreateConsultationRequest = {
  pharmacistId: number
  scheduledAt: string
  note?: string | null
}

export type UpdateConsultationStatusRequest = {
  status: ConsultationStatus
  responseNote?: string | null
}

export type ConsultationsListQuery = {
  page?: number
  pageSize?: number
  status?: ConsultationStatus
}

export type ConsultationsPaged = Paged<Consultation>
