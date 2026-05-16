// =============================================================================
// Pharmacist types - khop voi PharmaIntel.Core/DTOs/Pharmacists
// =============================================================================
import type { Paged } from '@/features/categories/types'

export type Pharmacist = {
  id: number
  fullName: string
  licenseNumber: string
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
  createdAt: string
  updatedAt: string
}

export type PharmacistListQuery = {
  page?: number
  pageSize?: number
  q?: string
  specialization?: string
  isOnline?: boolean
  isActive?: boolean
}

export type PharmacistsPaged = Paged<Pharmacist>
