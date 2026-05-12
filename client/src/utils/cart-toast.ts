// =============================================================================
// Toast helper cho cac loi cart/checkout.
// Map message backend (khong dau, vd "khong du ton kho") ve cau thong bao
// tieng Viet co dau cho user. Fallback ve nguyen message neu khong khop.
// =============================================================================
import { toast } from 'sonner'

export function showCartErrorToast(err: unknown, fallback = 'Không thực hiện được thao tác'): void {
  const msg = typeof err === 'string' ? err : fallback
  const lower = msg.toLowerCase()

  if (lower.includes('khong du ton kho') || lower.includes('vuot ton kho')) {
    toast.error('Sản phẩm vừa hết hàng hoặc không đủ số lượng. Vui lòng cập nhật giỏ hàng.')
    return
  }
  if (lower.includes('ngung kinh doanh')) {
    toast.error('Sản phẩm đã ngừng kinh doanh.')
    return
  }
  if (lower.includes('toi da 999')) {
    toast.error('Đã đạt số lượng tối đa cho sản phẩm này (999).')
    return
  }

  toast.error(msg)
}
