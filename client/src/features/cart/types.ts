// =============================================================================
// Cart types - khop voi PharmaIntel.Core/DTOs/Cart
// =============================================================================

export type CartItem = {
  id: number
  medicationId: number
  sku: string
  name: string
  imageUrl: string | null
  manufacturer: string | null
  unitPrice: number
  discountPercent: number
  finalUnitPrice: number
  quantity: number
  lineTotal: number
  isPrescriptionRequired: boolean
  stockQuantity: number
  isAvailable: boolean
  addedAt: string
}

export type Cart = {
  items: CartItem[]
  totalItems: number
  distinctItems: number
  subtotal: number
  totalDiscount: number
  total: number
  hasUnavailableItems: boolean
  hasPrescriptionRequired: boolean
}
