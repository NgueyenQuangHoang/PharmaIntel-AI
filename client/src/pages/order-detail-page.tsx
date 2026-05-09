// =============================================================================
// OrderDetailPage - Hien thi chi tiet 1 don hang cua user.
// Vao tu /orders/:id (sau khi checkout thanh cong, hoac tu lich su don).
// =============================================================================
import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import axios from 'axios'
import { ordersApi } from '@/features/orders/orders-api'
import type { OrderDto, OrderStatus, PaymentStatus } from '@/features/orders/types'
import { formatVnd } from '@/utils/format'

const STATUS_LABELS: Record<OrderStatus, string> = {
  pending: 'Chờ xác nhận',
  confirmed: 'Đã xác nhận',
  processing: 'Đang chuẩn bị',
  shipping: 'Đang giao',
  delivered: 'Đã giao',
  cancelled: 'Đã hủy',
  refunded: 'Đã hoàn tiền',
}

const PAYMENT_STATUS_LABELS: Record<PaymentStatus, string> = {
  unpaid: 'Chưa thanh toán',
  pending: 'Đang xử lý',
  paid: 'Đã thanh toán',
  failed: 'Thanh toán thất bại',
  refunded: 'Đã hoàn tiền',
  cod_pending: 'Sẽ thanh toán khi nhận hàng',
}

const PAYMENT_TYPE_LABELS: Record<string, string> = {
  cod: 'Thanh toán khi nhận hàng (COD)',
  bank_transfer: 'Chuyển khoản ngân hàng',
  momo: 'Ví MoMo',
  zalopay: 'ZaloPay',
  vnpay: 'VNPay',
  credit_card: 'Thẻ tín dụng',
}

function extractApiError(err: unknown, fallback: string): string {
  if (axios.isAxiosError(err)) {
    const data = err.response?.data as { title?: string; detail?: string; message?: string } | undefined
    return data?.detail ?? data?.title ?? data?.message ?? err.message ?? fallback
  }
  return fallback
}

export function OrderDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const orderId = id !== undefined && /^\d+$/.test(id) ? Number(id) : null
  const [order, setOrder] = useState<OrderDto | null>(null)
  const [loading, setLoading] = useState(orderId !== null)
  const [error, setError] = useState<string | null>(
    orderId === null ? 'Mã đơn hàng không hợp lệ' : null,
  )

  useEffect(() => {
    if (orderId === null) return
    let cancelled = false
    ordersApi
      .getById(orderId)
      .then((res) => {
        if (!cancelled) setOrder(res)
      })
      .catch((err) => {
        if (!cancelled) setError(extractApiError(err, 'Không tải được đơn hàng'))
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })
    return () => {
      cancelled = true
    }
  }, [orderId])

  if (loading) {
    return (
      <div className="pt-12 pb-24 px-4 md:px-12 max-w-5xl mx-auto">
        <div className="animate-pulse space-y-6">
          <div className="h-12 bg-surface-container-low rounded-lg w-1/2" />
          <div className="h-40 bg-surface-container-low rounded-xl" />
          <div className="h-60 bg-surface-container-low rounded-xl" />
        </div>
      </div>
    )
  }

  if (error || !order) {
    return (
      <div className="pt-12 pb-24 px-4 md:px-12 max-w-3xl mx-auto flex flex-col items-center justify-center min-h-[50vh]">
        <span className="material-symbols-outlined text-6xl text-error mb-4">error</span>
        <h2 className="text-2xl font-bold mb-2">Không tải được đơn hàng</h2>
        <p className="text-on-surface-variant mb-6 text-center">{error ?? 'Lỗi không xác định'}</p>
        <button
          onClick={() => navigate('/medicine')}
          className="px-6 py-3 bg-primary text-on-primary rounded-lg font-bold hover:bg-primary/90 transition-colors"
        >
          Trở lại Tủ thuốc
        </button>
      </div>
    )
  }

  const paymentTypeLabel = order.paymentTypeSnapshot
    ? PAYMENT_TYPE_LABELS[order.paymentTypeSnapshot] ?? order.paymentTypeSnapshot
    : '—'

  return (
    <div className="pt-12 pb-24 px-4 md:px-12 max-w-5xl mx-auto animate-in fade-in zoom-in-95 duration-500">
      {/* Success header */}
      <header className="mb-12 text-center">
        <div className="inline-flex items-center justify-center w-20 h-20 rounded-full bg-primary-container mb-4">
          <span
            className="material-symbols-outlined text-5xl text-on-primary-container"
            style={{ fontVariationSettings: "'FILL' 1" }}
          >
            check_circle
          </span>
        </div>
        <h1 className="text-4xl md:text-5xl font-extrabold tracking-tight text-on-surface mb-2 font-headline">
          Đặt hàng thành công!
        </h1>
        <p className="text-on-surface-variant font-medium">
          Mã đơn hàng:{' '}
          <span className="font-bold text-on-surface">{order.orderCode}</span>
        </p>
        <div className="mt-4 inline-flex items-center gap-2 px-4 py-2 rounded-full bg-secondary-container text-on-secondary-container font-semibold text-sm">
          <span
            className="material-symbols-outlined text-[18px]"
            style={{ fontVariationSettings: "'FILL' 1" }}
          >
            schedule
          </span>
          {STATUS_LABELS[order.status]}
        </div>
      </header>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Left: items + shipping */}
        <div className="lg:col-span-2 space-y-8">
          {/* Items */}
          <section className="bg-surface-container-lowest rounded-2xl p-6 border border-outline-variant/10">
            <h2 className="text-xl font-bold mb-6 font-headline flex items-center gap-2">
              <span className="material-symbols-outlined text-primary">medication</span>
              Sản phẩm ({order.itemCount})
            </h2>
            <div className="divide-y divide-outline-variant/20">
              {order.items.map((item) => {
                const finalUnit = Math.round(
                  item.unitPrice * (1 - Number(item.discountPercent) / 100),
                )
                return (
                  <div key={item.id} className="flex gap-4 py-4 first:pt-0 last:pb-0">
                    <div className="w-16 h-16 bg-surface-container-low rounded-lg flex items-center justify-center shrink-0">
                      <span className="material-symbols-outlined text-outline">medication</span>
                    </div>
                    <div className="flex-1 min-w-0">
                      <h3 className="font-bold leading-tight mb-1">{item.medicationName}</h3>
                      <div className="text-sm text-on-surface-variant">
                        {formatVnd(finalUnit)} × {item.quantity}
                        {Number(item.discountPercent) > 0 && (
                          <span className="ml-2 text-xs px-2 py-0.5 rounded-full bg-error-container text-on-error-container">
                            -{Number(item.discountPercent)}%
                          </span>
                        )}
                      </div>
                    </div>
                    <div className="font-bold text-primary text-right shrink-0">
                      {formatVnd(item.totalPrice)}
                    </div>
                  </div>
                )
              })}
            </div>
          </section>

          {/* QR thanh toan */}
          {order.vietQrUrl && (
            <section className="bg-surface-container-lowest rounded-2xl p-6 border border-outline-variant/10">
              <h2 className="text-xl font-bold mb-6 font-headline flex items-center gap-2">
                <span className="material-symbols-outlined text-primary">qr_code_2</span>
                Mã QR thanh toán
              </h2>
              <div className="flex justify-center">
                <div className="bg-white rounded-2xl p-3 shadow-sm border border-outline-variant/20 inline-block">
                  <img
                    src={order.vietQrUrl.replace(/-[^-.]+\.png/, '-qr_only.png')}
                    alt={`VietQR thanh toán đơn ${order.orderCode}`}
                    className="w-64 h-64 object-contain"
                    loading="lazy"
                  />
                </div>
              </div>
            </section>
          )}

          {/* Shipping info */}
          <section className="bg-surface-container-lowest rounded-2xl p-6 border border-outline-variant/10">
            <h2 className="text-xl font-bold mb-4 font-headline flex items-center gap-2">
              <span className="material-symbols-outlined text-primary">local_shipping</span>
              Thông tin giao hàng
            </h2>
            <div className="space-y-2 text-sm">
              <div className="flex gap-2">
                <span className="text-on-surface-variant w-28 shrink-0">Người nhận:</span>
                <span className="font-semibold">{order.shippingRecipientName ?? '—'}</span>
              </div>
              <div className="flex gap-2">
                <span className="text-on-surface-variant w-28 shrink-0">Số điện thoại:</span>
                <span className="font-semibold">{order.shippingPhone ?? '—'}</span>
              </div>
              <div className="flex gap-2">
                <span className="text-on-surface-variant w-28 shrink-0">Địa chỉ:</span>
                <span className="font-semibold">{order.shippingFullAddress ?? '—'}</span>
              </div>
            </div>
          </section>
        </div>

        {/* Right: pricing + payment */}
        <aside className="space-y-6">
          <section className="bg-surface-container-lowest rounded-2xl p-6 border border-outline-variant/10">
            <h2 className="text-xl font-bold mb-4 font-headline">Thanh toán</h2>
            <div className="space-y-3 text-sm">
              <div className="flex justify-between text-on-surface-variant">
                <span>Tạm tính</span>
                <span>{formatVnd(order.subtotal)}</span>
              </div>
              <div className="flex justify-between text-on-surface-variant">
                <span>Phí vận chuyển</span>
                <span>{formatVnd(order.shippingFee)}</span>
              </div>
              <div className="border-t border-outline-variant/20 pt-3 flex justify-between items-end">
                <span className="font-bold">Tổng cộng</span>
                <span className="text-2xl font-extrabold text-primary tracking-tighter">
                  {formatVnd(order.total)}
                </span>
              </div>
            </div>
            <div className="mt-6 pt-6 border-t border-outline-variant/20 space-y-2 text-sm">
              <div className="flex gap-2">
                <span className="text-on-surface-variant w-28 shrink-0">Phương thức:</span>
                <span className="font-semibold">{paymentTypeLabel}</span>
              </div>
              <div className="flex gap-2">
                <span className="text-on-surface-variant w-28 shrink-0">Trạng thái:</span>
                <span className="font-semibold">{PAYMENT_STATUS_LABELS[order.paymentStatus]}</span>
              </div>
            </div>
          </section>

          <button
            onClick={() => navigate('/medicine')}
            className="w-full py-4 bg-primary text-on-primary font-bold rounded-full hover:bg-primary/90 transition-colors flex items-center justify-center gap-2"
          >
            <span className="material-symbols-outlined">storefront</span>
            Tiếp tục mua sắm
          </button>
          <button
            onClick={() => navigate('/')}
            className="w-full py-4 bg-surface-container-high text-on-surface font-bold rounded-full hover:bg-surface-container-highest transition-colors"
          >
            Về trang chủ
          </button>
        </aside>
      </div>
    </div>
  )
}
