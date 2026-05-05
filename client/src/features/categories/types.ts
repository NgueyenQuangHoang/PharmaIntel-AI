// =============================================================================
// Category types - khop voi PharmaIntel.Core/DTOs/Categories
// =============================================================================

export type Category = {
  id: number
  name: string
  slug: string
  icon: string | null
  displayOrder: number
  isActive: boolean
  medicationCount: number
  createdAt: string
  updatedAt: string
}

export type CategoryListQuery = {
  page?: number
  pageSize?: number
  q?: string
  isActive?: boolean
}

export type Paged<T> = {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasNext: boolean
  hasPrevious: boolean
}
