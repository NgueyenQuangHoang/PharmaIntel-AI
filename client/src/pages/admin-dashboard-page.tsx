// =============================================================================
// AdminDashboardPage - trang tổng quan admin: 4 stat card + 2 chart + top SP.
// Dùng simple SVG/CSS bars để tránh thêm dependency (recharts).
// =============================================================================
import { useEffect } from 'react'
import { AdminPageShell } from '@/features/admin/components/AdminPageShell'
import { StatCard } from '@/features/admin/components/StatCard'
import { useAppDispatch, useAppSelector } from '@/hooks/redux'
import {
  fetchOrdersByStatusThunk,
  fetchOverviewThunk,
  fetchRevenueThunk,
  fetchTopMedicationsThunk,
} from '@/features/admin/admin-slice'
import { formatVnd } from '@/utils/format'
import type { RevenueRange } from '@/features/admin/types'

const STATUS_LABELS: Record<string, { label: string; tone: string }> = {
  pending: { label: 'Chờ duyệt', tone: 'bg-tertiary-container/60 text-tertiary' },
  confirmed: { label: 'Đã duyệt', tone: 'bg-primary-container/60 text-primary' },
  processing: { label: 'Đang chuẩn bị', tone: 'bg-secondary-container/60 text-secondary' },
  shipping: { label: 'Đang giao', tone: 'bg-secondary-container/60 text-secondary' },
  delivered: { label: 'Hoàn tất', tone: 'bg-primary-container/60 text-primary' },
  cancelled: { label: 'Đã hủy', tone: 'bg-error-container/60 text-error' },
  refunded: { label: 'Hoàn tiền', tone: 'bg-error-container/60 text-error' },
}

export function AdminDashboardPage() {
  const dispatch = useAppDispatch()
  const overview = useAppSelector((s) => s.admin.overview)
  const revenue = useAppSelector((s) => s.admin.revenue)
  const top = useAppSelector((s) => s.admin.topMedications)
  const ordersByStatus = useAppSelector((s) => s.admin.ordersByStatus)

  useEffect(() => {
    dispatch(fetchOverviewThunk())
    dispatch(fetchRevenueThunk('7d'))
    dispatch(fetchTopMedicationsThunk(5))
    dispatch(fetchOrdersByStatusThunk())
  }, [dispatch])

  const o = overview.data
  const maxRevenue = Math.max(1, ...revenue.data.map((p) => p.revenue))
  const totalOrdersInChart = ordersByStatus.data.reduce((s, x) => s + x.count, 0) || 1

  return (
    <AdminPageShell
      title="Tổng quan"
      description="Các chỉ số chính và xu hướng gần đây của hệ thống PharmaIntel."
    >
      {/* Stats grid */}
      <section className="grid gap-4 grid-cols-1 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard
          icon="group"
          label="Người dùng"
          value={o ? o.totalUsers.toLocaleString('vi-VN') : '—'}
          hint={o ? `${o.activeUsers} hoạt động · ${o.totalAdmins} quản trị` : undefined}
          tone="primary"
        />
        <StatCard
          icon="receipt_long"
          label="Tổng đơn hàng"
          value={o ? o.totalOrders.toLocaleString('vi-VN') : '—'}
          hint={o ? `${o.ordersPending} đang xử lý` : undefined}
          tone="secondary"
        />
        <StatCard
          icon="payments"
          label="Doanh thu"
          value={o ? formatVnd(o.totalRevenue) : '—'}
          hint={o ? `Hôm nay: ${formatVnd(o.revenueToday)} / ${o.ordersToday} đơn` : undefined}
          tone="tertiary"
        />
        <StatCard
          icon="medication"
          label="Sản phẩm"
          value={o ? o.totalMedications.toLocaleString('vi-VN') : '—'}
          hint={o ? `${o.totalCategories} danh mục` : undefined}
          tone="neutral"
        />
      </section>

      {overview.error && (
        <p className="mt-4 text-sm text-error bg-error-container/40 px-4 py-2 rounded-lg">
          {overview.error}
        </p>
      )}

      <div className="mt-6 grid gap-4 lg:grid-cols-3">
        {/* Revenue chart */}
        <section className="lg:col-span-2 rounded-2xl border border-outline-variant/40 bg-surface-container/60 p-6 backdrop-blur ambient-shadow">
          <div className="flex items-center justify-between mb-4">
            <div>
              <h2 className="font-headline text-lg font-bold text-on-surface">Doanh thu theo ngày</h2>
              <p className="text-xs text-on-surface-variant mt-1">
                Tổng: {formatVnd(revenue.data.reduce((s, p) => s + p.revenue, 0))}
              </p>
            </div>
            <RangeSelect
              value={revenue.range}
              onChange={(r) => dispatch(fetchRevenueThunk(r))}
            />
          </div>
          {revenue.status === 'loading' ? (
            <div className="h-48 flex items-center justify-center text-on-surface-variant">
              <span className="material-symbols-outlined animate-spin text-primary text-3xl">progress_activity</span>
            </div>
          ) : revenue.data.length === 0 ? (
            <p className="h-48 flex items-center justify-center text-on-surface-variant text-sm">
              Chưa có dữ liệu
            </p>
          ) : (
            <div className="flex items-end gap-1 h-48">
              {revenue.data.map((p) => {
                const h = (p.revenue / maxRevenue) * 100
                return (
                  <div key={p.date} className="flex-1 flex flex-col items-center justify-end group relative">
                    <div
                      className="w-full bg-gradient-to-t from-primary/80 to-primary/40 rounded-t transition-all hover:from-primary hover:to-primary/60"
                      style={{ height: `${Math.max(h, 2)}%` }}
                      title={`${p.date}: ${formatVnd(p.revenue)} (${p.orderCount} đơn)`}
                    />
                    <span className="mt-1 text-[10px] text-on-surface-variant whitespace-nowrap">
                      {p.date.slice(5)}
                    </span>
                  </div>
                )
              })}
            </div>
          )}
        </section>

        {/* Orders by status */}
        <section className="rounded-2xl border border-outline-variant/40 bg-surface-container/60 p-6 backdrop-blur ambient-shadow">
          <h2 className="font-headline text-lg font-bold text-on-surface mb-4">Trạng thái đơn hàng</h2>
          {ordersByStatus.status === 'loading' ? (
            <p className="text-sm text-on-surface-variant">Đang tải...</p>
          ) : ordersByStatus.data.length === 0 ? (
            <p className="text-sm text-on-surface-variant">Chưa có đơn hàng</p>
          ) : (
            <ul className="space-y-3">
              {ordersByStatus.data.map((s) => {
                const meta = STATUS_LABELS[s.status] ?? { label: s.status, tone: 'bg-surface-container-high text-on-surface-variant' }
                const pct = (s.count / totalOrdersInChart) * 100
                return (
                  <li key={s.status}>
                    <div className="flex items-center justify-between text-sm mb-1">
                      <span className={`whitespace-nowrap px-2 py-0.5 rounded-full text-xs font-semibold ${meta.tone}`}>
                        {meta.label}
                      </span>
                      <span className="text-on-surface-variant">
                        {s.count} ({pct.toFixed(0)}%)
                      </span>
                    </div>
                    <div className="h-1.5 rounded-full bg-surface-container-high overflow-hidden">
                      <div
                        className="h-full bg-primary"
                        style={{ width: `${pct}%` }}
                      />
                    </div>
                  </li>
                )
              })}
            </ul>
          )}
        </section>
      </div>

      {/* Top medications */}
      <section className="mt-6 rounded-2xl border border-outline-variant/40 bg-surface-container/60 p-6 backdrop-blur ambient-shadow">
        <h2 className="font-headline text-lg font-bold text-on-surface mb-4">Top sản phẩm bán chạy</h2>
        {top.status === 'loading' ? (
          <p className="text-sm text-on-surface-variant">Đang tải...</p>
        ) : top.data.length === 0 ? (
          <p className="text-sm text-on-surface-variant">Chưa có đơn hàng nào được giao thành công</p>
        ) : (
          <ul className="divide-y divide-outline-variant/30">
            {top.data.map((m, i) => (
              <li key={m.medicationId} className="py-3 flex items-center gap-3">
                <span className="w-7 h-7 rounded-full bg-primary-container/60 text-primary text-sm font-bold flex items-center justify-center">
                  {i + 1}
                </span>
                {m.imageUrl ? (
                  <img src={m.imageUrl} alt="" className="w-10 h-10 rounded-lg object-cover" />
                ) : (
                  <span className="w-10 h-10 rounded-lg bg-surface-container-high flex items-center justify-center">
                    <span className="material-symbols-outlined text-on-surface-variant">medication</span>
                  </span>
                )}
                <div className="flex-1 min-w-0">
                  <p className="font-semibold text-on-surface truncate">{m.name}</p>
                  <p className="text-xs text-on-surface-variant">
                    {m.quantitySold} đã bán · {formatVnd(m.revenue)}
                  </p>
                </div>
              </li>
            ))}
          </ul>
        )}
      </section>
    </AdminPageShell>
  )
}

function RangeSelect({ value, onChange }: { value: RevenueRange; onChange: (r: RevenueRange) => void }) {
  const opts: { value: RevenueRange; label: string }[] = [
    { value: '7d', label: '7 ngày' },
    { value: '30d', label: '30 ngày' },
    { value: '90d', label: '90 ngày' },
  ]
  return (
    <div className="inline-flex rounded-full border border-outline-variant/40 bg-surface-container-low p-0.5">
      {opts.map((o) => (
        <button
          key={o.value}
          type="button"
          onClick={() => onChange(o.value)}
          className={`px-3 py-1 text-xs font-semibold rounded-full transition-colors ${
            value === o.value ? 'bg-primary text-on-primary' : 'text-on-surface-variant hover:text-on-surface'
          }`}
        >
          {o.label}
        </button>
      ))}
    </div>
  )
}
