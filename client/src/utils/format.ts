// =============================================================================
// Format helpers
// =============================================================================

export function formatVnd(amount: number): string {
  // Backend tra ve so VND nguyen, dung "đ" thay vi "₫" cho gan voi UI hien tai.
  return new Intl.NumberFormat('vi-VN', {
    maximumFractionDigits: 0,
  }).format(amount) + 'đ'
}

export function calcFinalPrice(price: number, discountPercent: number): number {
  return Math.round(price * (1 - discountPercent / 100))
}
