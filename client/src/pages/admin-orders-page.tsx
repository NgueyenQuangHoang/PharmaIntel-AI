// =============================================================================
// AdminOrdersPage - liệt kê đơn hàng toàn hệ thống + cập nhật status.
// State machine BE enforce: pending -> confirmed -> processing -> shipping ->
//                          delivered (terminal); cancelled/refunded là nhánh phụ.
// FE chỉ gửi status mới, BE tự reject nếu transition không hợp lệ.
// =============================================================================
import { useEffect, useState } from 'react'
import { AdminPageShell } from '@/features/admin/components/AdminPageShell'
import { DataTable, type DataTableColumn } from '@/features/admin/components/DataTable'
import { ConfirmDialog } from '@/features/admin/components/ConfirmDialog'
import { adminApi, type AdminOrderListQuery } from '@/features/admin/admin-api'
import type { AdminOrderItem } from '@/features/admin/types'
import { formatVnd } from '@/utils/format'

const ALL_STATUSES = [
  'pending',
  'confirmed',
  'processing',
  'shipping',
  'delivered',
  'cancelled',
  'refunded',
] as const

const STATUS_META: Record<string, { label: string; tone: string }> = {
  pending: { label: 'Chờ duyệt', tone: 'bg-tertiary-container/60 text-tertiary' },
  confirmed: { label: 'Đã duyệt', tone: 'bg-primary-container/60 text-primary' },
  processing: { label: 'Đang chuẩn bị', tone: 'bg-secondary-container/60 text-secondary' },
  shipping: { label: 'Đang giao', tone: 'bg-secondary-container/60 text-secondary' },
  delivered: { label: 'Hoàn tất', tone: 'bg-primary-container/60 text-primary' },
  cancelled: { label: 'Đã hủy', tone: 'bg-error-container/60 text-error' },
  refunded: { label: 'Hoàn tiền', tone: 'bg-error-container/60 text-error' },
}

const PAYMENT_META: Record<string, { label: string; tone: string }> = {
  unpaid: { label: 'Chưa TT', tone: 'bg-surface-container-high text-on-surface-variant' },
  pending: { label: 'Chờ TT', tone: 'bg-tertiary-container/60 text-tertiary' },
  paid: { label: 'Đã TT', tone: 'bg-primary-container/60 text-primary' },
  failed: { label: 'TT lỗi', tone: 'bg-error-container/60 text-error' },
  refunded: { label: 'Hoàn tiền', tone: 'bg-error-container/60 text-error' },
  cod_pending: { label: 'COD chờ', tone: 'bg-tertiary-container/60 text-tertiary' },
}

type ChangeStatus = { order: AdminOrderItem; newStatus: string }

export function AdminOrdersPage() {
  const [items, setItems] = useState<AdminOrderItem[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [filter, setFilter] = useState<AdminOrderListQuery>({ page: 1, pageSize: 20 })
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [change, setChange] = useState<ChangeStatus | null>(null)
  const [busy, setBusy] = useState(false)
  const [actionError, setActionError] = useState<string | null>(null)

  async function load() {
    setLoading(true)
    setError(null)
    try {
      const paged = await adminApi.orders.list(filter)
      setItems(paged.items)
      setTotalCount(paged.totalCount)
    } catch (err: unknown) {
      const e = err as { response?: { data?: { detail?: string } }; message?: string }
      setError(e.response?.data?.detail ?? e.message ?? 'Không tải được danh sách đơn hàng')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    load()
  }, [filter])

  async function executeChange() {
    if (!change) return
    setBusy(true)
    setActionError(null)
    try {
      await adminApi.orders.updateStatus(change.order.id, change.newStatus)
      setChange(null)
      await load()
    } catch (err: unknown) {
      const e = err as { response?: { data?: { detail?: string; title?: string } } }
      setActionError(
        e.response?.data?.detail ?? e.response?.data?.title ?? 'Lỗi cập nhật trạng thái',
      )
    } finally {
      setBusy(false)
    }
  }

  const columns: DataTableColumn<AdminOrderItem>[] = [
    {
      key: 'order',
      header: 'Đơn hàng',
      cell: (o) => (
        <div>
          <p className="font-mono text-sm font-semibold text-on-surface">{o.orderCode}</p>
          <p className="text-xs text-on-surface-variant">
            {new Date(o.createdAt).toLocaleString('vi-VN')}
          </p>
        </div>
      ),
    },
    {
      key: 'customer',
      header: 'Khách hàng',
      cell: (o) => (
        <div className="min-w-0">
          <p className="font-semibold text-on-surface truncate">{o.userFullName ?? `User #${o.userId}`}</p>
          {o.userEmail && <p className="text-xs text-on-surface-variant truncate">{o.userEmail}</p>}
        </div>
      ),
    },
    {
      key: 'total',
      header: 'Tổng',
      cell: (o) => <span className="text-sm font-semibold">{formatVnd(o.total)}</span>,
    },
    {
      key: 'payment',
      header: 'Thanh toán',
      cell: (o) => {
        const m = PAYMENT_META[o.paymentStatus] ?? {
          label: o.paymentStatus,
          tone: 'bg-surface-container-high text-on-surface-variant',
        }
        return (
          <span className={`whitespace-nowrap px-2 py-0.5 rounded-full text-xs font-semibold ${m.tone}`}>
            {m.label}
          </span>
        )
      },
    },
    {
      key: 'status',
      header: 'Trạng thái',
      cell: (o) => {
        const m = STATUS_META[o.status] ?? {
          label: o.status,
          tone: 'bg-surface-container-high text-on-surface-variant',
        }
        return (
          <select
            value={o.status}
            onChange={(e) => {
              const newStatus = e.target.value
              if (newStatus !== o.status) {
                setChange({ order: o, newStatus })
              }
            }}
            className={`rounded-full px-2 py-0.5 text-xs font-semibold outline-none border-0 cursor-pointer ${m.tone}`}
          >
            {ALL_STATUSES.map((s) => (
              <option key={s} value={s} className="bg-surface text-on-surface">
                {STATUS_META[s]?.label ?? s}
              </option>
            ))}
          </select>
        )
      },
    },
  ]

  const totalPages = Math.max(1, Math.ceil(totalCount / (filter.pageSize ?? 20)))

  return (
    <AdminPageShell
      title="Quản lý đơn hàng"
      description={`Tổng: ${totalCount.toLocaleString('vi-VN')} đơn hàng`}
    >
      <div className="mb-4 flex flex-wrap gap-2">
        <select
          value={filter.status ?? ''}
          onChange={(e) =>
            setFilter((f) => ({ ...f, status: e.target.value || undefined, page: 1 }))
          }
          className="rounded-xl border border-outline-variant/40 bg-surface-container-low px-3 py-2 text-sm text-on-surface outline-none"
        >
          <option value="">Tất cả trạng thái</option>
          {ALL_STATUSES.map((s) => (
            <option key={s} value={s}>
              {STATUS_META[s].label}
            </option>
          ))}
        </select>
        <select
          value={filter.paymentStatus ?? ''}
          onChange={(e) =>
            setFilter((f) => ({ ...f, paymentStatus: e.target.value || undefined, page: 1 }))
          }
          className="rounded-xl border border-outline-variant/40 bg-surface-container-low px-3 py-2 text-sm text-on-surface outline-none"
        >
          <option value="">Tất cả thanh toán</option>
          {Object.entries(PAYMENT_META).map(([key, meta]) => (
            <option key={key} value={key}>
              {meta.label}
            </option>
          ))}
        </select>
      </div>

      {error && (
        <p className="mb-3 text-sm text-error bg-error-container/40 px-4 py-2 rounded-lg">{error}</p>
      )}

      <DataTable<AdminOrderItem>
        columns={columns}
        rows={items}
        rowKey={(o) => o.id}
        loading={loading}
        emptyMessage="Không tìm thấy đơn hàng"
        footer={
          <div className="flex items-center justify-between text-sm text-on-surface-variant">
            <span>
              Trang {filter.page ?? 1} / {totalPages}
            </span>
            <div className="flex gap-1">
              <button
                type="button"
                disabled={(filter.page ?? 1) <= 1}
                onClick={() => setFilter((f) => ({ ...f, page: (f.page ?? 1) - 1 }))}
                className="px-3 py-1.5 rounded-lg border border-outline-variant/40 disabled:opacity-30 hover:bg-surface-container-high"
              >
                Trước
              </button>
              <button
                type="button"
                disabled={(filter.page ?? 1) >= totalPages}
                onClick={() => setFilter((f) => ({ ...f, page: (f.page ?? 1) + 1 }))}
                className="px-3 py-1.5 rounded-lg border border-outline-variant/40 disabled:opacity-30 hover:bg-surface-container-high"
              >
                Sau
              </button>
            </div>
          </div>
        }
      />

      <ConfirmDialog
        open={!!change}
        busy={busy}
        title="Cập nhật trạng thái đơn hàng"
        message={
          change ? (
            <>
              <p>
                Đổi trạng thái đơn <b>{change.order.orderCode}</b> từ{' '}
                <b>{STATUS_META[change.order.status]?.label ?? change.order.status}</b> sang{' '}
                <b>{STATUS_META[change.newStatus]?.label ?? change.newStatus}</b>?
              </p>
              <p className="mt-2 text-xs text-on-surface-variant">
                BE sẽ tự reject nếu transition không hợp lệ theo state machine.
              </p>
              {actionError && (
                <p className="mt-3 text-error text-sm bg-error-container/40 px-3 py-2 rounded">
                  {actionError}
                </p>
              )}
            </>
          ) : null
        }
        onCancel={() => {
          setChange(null)
          setActionError(null)
        }}
        onConfirm={executeChange}
      />
    </AdminPageShell>
  )
}
