// =============================================================================
// Order types - khop voi PharmaIntel.Core/DTOs/Orders
// =============================================================================

export type OrderStatus =
  | 'pending'
  | 'confirmed'
  | 'processing'
  | 'shipping'
  | 'delivered'
  | 'cancelled'
  | 'refunded'

export type PaymentStatus =
  | 'unpaid'
  | 'pending'
  | 'paid'
  | 'failed'
  | 'refunded'
  | 'cod_pending'

export type OrderItemDto = {
  id: number
  medicationId: number | null
  medicationName: string
  quantity: number
  unitPrice: number
  discountPercent: number
  totalPrice: number
}

export type OrderListItemDto = {
  id: number
  orderCode: string
  subtotal: number
  shippingFee: number
  total: number
  status: OrderStatus
  paymentStatus: PaymentStatus
  paymentTypeSnapshot: string | null
  itemCount: number
  createdAt: string
}

export type OrderDto = OrderListItemDto & {
  addressId: number | null
  paymentMethodId: number | null
  shippingRecipientName: string | null
  shippingPhone: string | null
  shippingFullAddress: string | null
  updatedAt: string
  items: OrderItemDto[]
  vietQrUrl: string | null         // co gia tri khi paymentTypeSnapshot = 'bank_transfer' va don chua paid
  transferContent: string | null
}

export type CheckoutPaymentType = 'cod' | 'bank_transfer'

export type CheckoutRequest = {
  addressId: number
  paymentMethodId?: number | null  // null/omit -> backend ensure-or-create theo paymentType
  paymentType?: CheckoutPaymentType  // 'cod' (default) | 'bank_transfer'
  prescriptionId?: number | null
}
