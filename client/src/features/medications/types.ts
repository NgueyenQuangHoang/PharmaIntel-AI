// =============================================================================
// Medication types - khop voi PharmaIntel.Core/DTOs/Medications
// =============================================================================
import type { Paged } from '@/features/categories/types'

export type MedicationListItem = {
  id: number
  sku: string
  name: string
  genericName: string | null
  manufacturer: string | null
  price: number
  discountPercent: number
  finalPrice: number
  categoryId: number
  categoryName: string
  imageUrl: string | null
  isFeatured: boolean
  isBestSeller: boolean
  isPrescriptionRequired: boolean
  stockQuantity: number
  isActive: boolean
}

export type Medication = MedicationListItem & {
  registrationNumber: string | null
  description: string | null
  dosage: string | null
  packaging: string | null
  usageInstructions: string | null
  benefits: string | null
  activeIngredients: string | null
  contraindications: string | null
  sideEffects: string | null
  storageInstructions: string | null
  createdAt: string
  updatedAt: string
}

export type MedicationListQuery = {
  page?: number
  pageSize?: number
  q?: string
  categoryId?: number | null
  isActive?: boolean
  isFeatured?: boolean
  isBestSeller?: boolean
  isPrescriptionRequired?: boolean
  minPrice?: number
  maxPrice?: number
  sortBy?: 'name' | 'price' | 'createdAt'
  sortDesc?: boolean
}

export type MedicationsPaged = Paged<MedicationListItem>
