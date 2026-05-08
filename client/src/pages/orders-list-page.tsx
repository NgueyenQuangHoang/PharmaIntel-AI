// =============================================================================
// OrdersListPage - Lich su don hang cua user dang dang nhap.
// Filter theo status, phan trang. Click row -> /orders/:id.
// =============================================================================
import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import axios from 'axios'
import { ordersApi } from '@/features/orders/orders-api'
import type { OrderListItemDto, OrderStatus, PaymentStatus } from '@/features/orders/types'
import { formatVnd } from '@/utils/format'

const STATUS_FILTERS: { value: '' | OrderStatus; label: string }[] = [
  { value: '', label: 'Tất cả' },
  { value: 'pending', label: 'Chờ xác nhận' },
  { value: 'confirmed', label: 'Đã xác nhận' },
  { value: 'processing', label: 'Đang chuẩn bị' },
  { value: 'shipping', label: 'Đang giao' },
  { value: 'delivered', label: 'Đã giao' },
  { value: 'cancelled', label: 'Đã hủy' },
]

const STATUS_BADGE: Record<OrderStatus, { label: string; className: string }> = {
  pending: { label: 'Chờ xác nhận', className: 'bg-amber-100 text-amber-800' },
  confirmed: { label: 'Đã xác nhận', className: 'bg-blue-100 text-blue-800' },
  processing: { label: 'Đang chuẩn bị', className: 'bg-indigo-100 text-indigo-800' },
  shipping: { label: 'Đang giao', className: 'bg-cyan-100 text-cyan-800' },
  delivered: { label: 'Đã giao', className: 'bg-emerald-100 text-emerald-800' },
  cancelled: { label: 'Đã hủy', className: 'bg-rose-100 text-rose-800' },
  refunded: { label: 'Đã hoàn tiền', className: 'bg-slate-200 text-slate-700' },
}

const PAYMENT_LABEL: Record<PaymentStatus, string> = {
  unpaid: 'Chưa thanh toán',
  pending: 'Đang xử lý',
  paid: 'Đã thanh toán',
  failed: 'Thất bại',
  refunded: 'Đã hoàn tiền',
  cod_pending: 'COD',
}

function extractApiError(err: unknown, fallback: string): string {
  if (axios.isAxiosError(err)) {
    const data = err.response?.data as { title?: string; detail?: string; message?: string } | undefined
    return data?.detail ?? data?.title ?? data?.message ?? err.message ?? fallback
  }
  return fallback
}

function formatDate(iso: string): string {
  try {
    const d = new Date(iso)
    return d.toLocaleDateString('vi-VN', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    })
  } catch {
    return iso
  }
}

export function OrdersListPage() {
  const navigate = useNavigate()
  const [status, setStatus] = useState<'' | OrderStatus>('')
  const [page, setPage] = useState(1)
  const [orders, setOrders] = useState<OrderListItemDto[]>([])
  const [totalPages, setTotalPages] = useState(0)
  const [totalCount, setTotalCount] = useState(0)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let cancelled = false
    ordersApi
      .listMy({ page, pageSize: 10, status: status || undefined })
      .then((res) => {
        if (cancelled) return
        setOrders(res.items)
        setTotalPages(res.totalPages)
        setTotalCount(res.totalCount)
        setError(null)
      })
      .catch((err) => {
        if (!cancelled) setError(extractApiError(err, 'Không tải được danh sách đơn'))
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })
    return () => {
      cancelled = true
    }
  }, [page, status])

  return (
    <div className="pt-12 pb-24 px-4 md:px-12 max-w-6xl mx-auto animate-in fade-in zoom-in-95 duration-500">
      <header className="mb-8">
        <h1 className="text-4xl font-extrabold tracking-tight text-on-surface mb-2 font-headline">
          Đơn hàng của tôi
        </h1>
        <p className="text-on-surface-variant font-medium">
          {totalCount > 0 ? `Tổng: ${totalCount} đơn hàng` : 'Lịch sử các đơn đã đặt'}
        </p>
      </header>

      {/* Filter chips */}
      <div className="flex flex-wrap gap-2 mb-6">
        {STATUS_FILTERS.map((f) => {
          const active = status === f.value
          return (
            <button
              key={f.value || 'all'}
              onClick={() => {
                setStatus(f.value)
                setPage(1)
              }}
              className={`px-4 py-2 rounded-full text-sm font-semibold transition-colors ${
                active
                  ? 'bg-primary text-on-primary'
                  : 'bg-surface-container-low text-on-surface-variant hover:bg-surface-container'
              }`}
            >
              {f.label}
            </button>
          )
        })}
      </div>

      {/* Error */}
      {error && (
        <div className="mb-4 p-4 rounded-xl bg-error-container text-on-error-container font-medium">
          {error}
        </div>
      )}

      {/* Loading */}
      {loading && (
        <div className="space-y-3">
          {[1, 2, 3].map((i) => (
            <div key={i} className="h-24 bg-surface-container-low rounded-xl animate-pulse" />
          ))}
        </div>
      )}

      {/* Empty */}
      {!loading && !error && orders.length === 0 && (
        <div className="flex flex-col items-center justify-center py-20 text-center">
          <span className="material-symbols-outlined text-6xl text-outline mb-4">receipt_long</span>
          <h2 className="text-2xl font-bold mb-2">Chưa có đơn hàng nào</h2>
          <p className="text-on-surface-variant mb-6">
            Hãy bắt đầu mua thuốc đầu tiên của bạn.
          </p>
          <button
            onClick={() => navigate('/medicine')}
            className="px-6 py-3 bg-primary text-on-primary rounded-lg font-bold hover:bg-primary/90 transition-colors"
          >
            Khám phá Tủ thuốc
          </button>
        </div>
      )}

      {/* Order list */}
      {!loading && orders.length > 0 && (
        <div className="space-y-3">
          {orders.map((o) => {
            const badge = STATUS_BADGE[o.status]
            return (
              <button
                key={o.id}
                onClick={() => navigate(`/orders/${o.id}`)}
                className="w-full text-left bg-surface-container-lowest hover:bg-surface-container-low border border-outline-variant/20 rounded-xl p-5 transition-colors"
              >
                <div className="flex flex-wrap items-start justify-between gap-4">
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-3 mb-2">
                      <span className="font-bold text-base">{o.orderCode}</span>
                      <span
                        className={`text-xs px-2 py-0.5 rounded-full font-semibold ${badge.className}`}
                      >
                        {badge.label}
                      </span>
                    </div>
                    <div className="text-sm text-on-surface-variant space-y-0.5">
                      <div>{formatDate(o.createdAt)}</div>
                      <div>
                        {o.itemCount} sản phẩm · {PAYMENT_LABEL[o.paymentStatus]}
                      </div>
                    </div>
                  </div>
                  <div className="text-right shrink-0">
                    <div className="text-xs text-on-surface-variant mb-1">Tổng cộng</div>
                    <div className="text-xl font-extrabold text-primary tracking-tighter">
                      {formatVnd(o.total)}
                    </div>
                  </div>
                </div>
              </button>
            )
          })}
        </div>
      )}

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex items-center justify-center gap-2 mt-8">
          <button
            disabled={page <= 1}
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            className="px-4 py-2 rounded-lg bg-surface-container-low text-on-surface font-semibold disabled:opacity-40 disabled:cursor-not-allowed hover:bg-surface-container transition-colors"
          >
            Trước
          </button>
          <span className="px-4 text-sm text-on-surface-variant font-medium">
            Trang {page} / {totalPages}
          </span>
          <button
            disabled={page >= totalPages}
            onClick={() => setPage((p) => p + 1)}
            className="px-4 py-2 rounded-lg bg-surface-container-low text-on-surface font-semibold disabled:opacity-40 disabled:cursor-not-allowed hover:bg-surface-container transition-colors"
          >
            Sau
          </button>
        </div>
      )}
    </div>
  )
}
