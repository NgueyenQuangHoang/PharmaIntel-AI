// =============================================================================
// Address types - khop voi PharmaIntel.Core/DTOs/Addresses
// =============================================================================

export type AddressDto = {
  id: number
  userId: number
  recipientName: string
  phone: string
  province: string
  district?: string
  ward: string
  streetAddress: string
  fullAddress: string
  isDefault: boolean
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export type AddressCreateRequest = {
  recipientName: string
  phone: string
  province: string
  district?: string
  ward: string
  streetAddress: string
  isDefault?: boolean
}

export type PagedResult<T> = {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasNext: boolean
  hasPrevious: boolean
}
