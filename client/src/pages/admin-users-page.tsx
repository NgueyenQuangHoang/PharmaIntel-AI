// =============================================================================
// AdminUsersPage - quản lý người dùng: list, filter, đổi role, lock/unlock, xóa.
// Tất cả thao tác mutating gọi adminApi trực tiếp + refetch list.
// =============================================================================
import { useEffect, useState } from 'react'
import { AdminPageShell } from '@/features/admin/components/AdminPageShell'
import { DataTable, type DataTableColumn } from '@/features/admin/components/DataTable'
import { ConfirmDialog } from '@/features/admin/components/ConfirmDialog'
import { useAppDispatch, useAppSelector } from '@/hooks/redux'
import { fetchAdminUsersThunk } from '@/features/admin/admin-slice'
import { adminApi } from '@/features/admin/admin-api'
import type { AdminUser, AdminUserListQuery } from '@/features/admin/types'
import { formatVnd } from '@/utils/format'
import { useAuth } from '@/hooks/useAuth'

type ConfirmAction =
  | { kind: 'role'; user: AdminUser; nextRole: 'user' | 'admin' | 'pharmacist'; licenseNumber?: string }
  | { kind: 'lock'; user: AdminUser; nextActive: boolean }
  | { kind: 'delete'; user: AdminUser }

export function AdminUsersPage() {
  const dispatch = useAppDispatch()
  const users = useAppSelector((s) => s.admin.users)
  const { user: currentUser } = useAuth()

  const [filter, setFilter] = useState<AdminUserListQuery>({ page: 1, pageSize: 20 })
  const [searchInput, setSearchInput] = useState('')
  const [confirm, setConfirm] = useState<ConfirmAction | null>(null)
  const [busy, setBusy] = useState(false)
  const [actionError, setActionError] = useState<string | null>(null)

  useEffect(() => {
    dispatch(fetchAdminUsersThunk(filter))
  }, [dispatch, filter])

  useEffect(() => {
    const t = setTimeout(() => {
      setFilter((f) => {
        const q = searchInput.trim() || undefined
        if (f.q === q) return f
        return { ...f, q, page: 1 }
      })
    }, 300)
    return () => clearTimeout(t)
  }, [searchInput])

  function applySearch() {
    setFilter((f) => ({ ...f, q: searchInput.trim() || undefined, page: 1 }))
  }

  async function executeConfirm() {
    if (!confirm) return
    setBusy(true)
    setActionError(null)
    try {
      if (confirm.kind === 'role') {
        await adminApi.users.updateRole(confirm.user.id, { 
          role: confirm.nextRole,
          licenseNumber: confirm.licenseNumber
        })
      } else if (confirm.kind === 'lock') {
        await adminApi.users.updateStatus(confirm.user.id, { isActive: confirm.nextActive })
      } else {
        await adminApi.users.delete(confirm.user.id)
      }
      setConfirm(null)
      dispatch(fetchAdminUsersThunk(filter))
    } catch (err: unknown) {
      const e = err as { response?: { data?: { detail?: string; title?: string; message?: string } } }
      setActionError(
        e.response?.data?.detail ?? e.response?.data?.title ?? e.response?.data?.message ?? 'Lỗi thao tác',
      )
    } finally {
      setBusy(false)
    }
  }

  const columns: DataTableColumn<AdminUser>[] = [
    {
      key: 'user',
      header: 'Người dùng',
      cell: (u) => (
        <div className="flex items-center gap-3">
          {u.avatarUrl ? (
            <img src={u.avatarUrl} alt="" className="w-9 h-9 rounded-full object-cover" />
          ) : (
            <span className="w-9 h-9 rounded-full bg-primary-container/60 text-primary flex items-center justify-center text-sm font-bold">
              {u.fullName.charAt(0).toUpperCase()}
            </span>
          )}
          <div className="min-w-0">
            <p className="font-semibold text-on-surface truncate">{u.fullName}</p>
            <p className="text-xs text-on-surface-variant truncate">{u.email}</p>
          </div>
        </div>
      ),
    },
    {
      key: 'role',
      header: 'Vai trò',
      cell: (u) => {
        let label = 'Người dùng'
        let colorClass = 'bg-surface-container-high text-on-surface-variant'
        if (u.role === 'admin') {
          label = 'Quản trị'
          colorClass = 'bg-tertiary-container/60 text-tertiary'
        } else if (u.role === 'pharmacist') {
          label = 'Dược sĩ'
          colorClass = 'bg-secondary-container/60 text-secondary'
        }
        return (
          <span className={`whitespace-nowrap px-2 py-0.5 rounded-full text-xs font-semibold ${colorClass}`}>
            {label}
          </span>
        )
      },
    },
    {
      key: 'status',
      header: 'Trạng thái',
      cell: (u) => (
        <span
          className={`whitespace-nowrap px-2 py-0.5 rounded-full text-xs font-semibold ${
            u.isActive
              ? 'bg-primary-container/60 text-primary'
              : 'bg-error-container/60 text-error'
          }`}
        >
          {u.isActive ? 'Hoạt động' : 'Đã khóa'}
        </span>
      ),
    },
    {
      key: 'orders',
      header: 'Đơn',
      cell: (u) => <span className="text-sm">{u.totalOrders}</span>,
    },
    {
      key: 'spent',
      header: 'Chi tiêu',
      cell: (u) => <span className="text-sm">{formatVnd(u.totalSpent)}</span>,
    },
    {
      key: 'actions',
      header: '',
      headerClassName: 'text-right',
      className: 'text-right',
      cell: (u) => {
        const isSelf = currentUser?.id === u.id
        return (
          <div className="flex items-center justify-end gap-1">
            <select
              value={u.role || 'user'}
              disabled={isSelf}
              onChange={(e) => setConfirm({ kind: 'role', user: u, nextRole: e.target.value as 'user' | 'admin' | 'pharmacist' })}
              className="px-2 py-1 text-sm rounded-lg border border-outline-variant/40 bg-surface-container-low outline-none disabled:opacity-30 disabled:cursor-not-allowed cursor-pointer"
            >
              <option value="user">Người dùng</option>
              <option value="admin">Quản trị</option>
              <option value="pharmacist">Dược sĩ</option>
            </select>
            <button
              type="button"
              disabled={isSelf}
              onClick={() => setConfirm({ kind: 'lock', user: u, nextActive: !u.isActive })}
              title={isSelf ? 'Không thể khóa chính mình' : u.isActive ? 'Khóa' : 'Mở khóa'}
              className="p-1.5 rounded-lg text-on-surface-variant hover:bg-surface-container-high disabled:opacity-30 disabled:cursor-not-allowed"
            >
              <span className="material-symbols-outlined text-[18px]">{u.isActive ? 'lock' : 'lock_open'}</span>
            </button>
            <button
              type="button"
              disabled={isSelf}
              onClick={() => setConfirm({ kind: 'delete', user: u })}
              title={isSelf ? 'Không thể xóa chính mình' : 'Xóa'}
              className="p-1.5 rounded-lg text-error hover:bg-error-container/40 disabled:opacity-30 disabled:cursor-not-allowed"
            >
              <span className="material-symbols-outlined text-[18px]">delete</span>
            </button>
          </div>
        )
      },
    },
  ]

  const totalPages = Math.max(1, Math.ceil(users.totalCount / users.pageSize))

  return (
    <AdminPageShell
      title="Quản lý người dùng"
      description={`Tổng: ${users.totalCount.toLocaleString('vi-VN')} người dùng`}
    >
      {/* Filters */}
      <div className="mb-4 flex flex-col sm:flex-row gap-2">
        <div className="flex flex-1 items-center gap-2 rounded-xl border border-outline-variant/40 bg-surface-container-low px-3 py-2 input-ghost-border">
          {users.status === 'loading' ? (
            <span className="material-symbols-outlined text-primary text-[20px] animate-spin">
              progress_activity
            </span>
          ) : (
            <span className="material-symbols-outlined text-on-surface-variant text-[20px]">
              search
            </span>
          )}
          <input
            type="text"
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && applySearch()}
            placeholder="Tìm tên hoặc email..."
            className="flex-1 bg-transparent outline-none text-sm text-on-surface placeholder:text-on-surface-variant"
          />
          {searchInput && (
            <button
              type="button"
              onClick={() => {
                setSearchInput('')
                setFilter((f) => ({ ...f, q: undefined, page: 1 }))
              }}
              className="text-on-surface-variant hover:text-on-surface"
            >
              <span className="material-symbols-outlined text-[18px]">close</span>
            </button>
          )}
        </div>
        <select
          value={filter.role ?? ''}
          onChange={(e) => setFilter((f) => ({ ...f, role: (e.target.value || undefined) as 'user' | 'admin' | 'pharmacist' | undefined, page: 1 }))}
          className="rounded-xl border border-outline-variant/40 bg-surface-container-low px-3 py-2 text-sm text-on-surface outline-none"
        >
          <option value="">Tất cả vai trò</option>
          <option value="user">Người dùng</option>
          <option value="admin">Quản trị</option>
          <option value="pharmacist">Dược sĩ</option>
        </select>
        <select
          value={filter.isActive == null ? '' : String(filter.isActive)}
          onChange={(e) => {
            const v = e.target.value
            setFilter((f) => ({
              ...f,
              isActive: v === '' ? undefined : v === 'true',
              page: 1,
            }))
          }}
          className="rounded-xl border border-outline-variant/40 bg-surface-container-low px-3 py-2 text-sm text-on-surface outline-none"
        >
          <option value="">Tất cả trạng thái</option>
          <option value="true">Hoạt động</option>
          <option value="false">Đã khóa</option>
        </select>
        <button
          type="button"
          onClick={applySearch}
          className="rounded-xl bg-primary text-on-primary px-4 py-2 text-sm font-semibold hover:bg-primary/90"
        >
          Lọc
        </button>
      </div>

      {users.error && (
        <p className="mb-3 text-sm text-error bg-error-container/40 px-4 py-2 rounded-lg">{users.error}</p>
      )}

      <DataTable<AdminUser>
        columns={columns}
        rows={users.items}
        rowKey={(u) => u.id}
        loading={users.status === 'loading'}
        emptyMessage="Không tìm thấy người dùng phù hợp"
        footer={
          <div className="flex items-center justify-between text-sm text-on-surface-variant">
            <span>
              Trang {users.page} / {totalPages}
            </span>
            <div className="flex gap-1">
              <button
                type="button"
                disabled={users.page <= 1}
                onClick={() => setFilter((f) => ({ ...f, page: (f.page ?? 1) - 1 }))}
                className="px-3 py-1.5 rounded-lg border border-outline-variant/40 disabled:opacity-30 hover:bg-surface-container-high"
              >
                Trước
              </button>
              <button
                type="button"
                disabled={users.page >= totalPages}
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
        open={!!confirm}
        busy={busy}
        title={
          confirm?.kind === 'role'
            ? 'Đổi vai trò'
            : confirm?.kind === 'lock'
              ? confirm.nextActive
                ? 'Mở khóa tài khoản'
                : 'Khóa tài khoản'
              : 'Xóa người dùng'
        }
        variant={confirm?.kind === 'delete' ? 'danger' : 'primary'}
        message={
          confirm ? (
            <>
              <p>
                {confirm.kind === 'role' && (
                  <>
                    <p>Đổi vai trò của <b>{confirm.user.fullName}</b> thành <b>{confirm.nextRole}</b>?</p>
                    {confirm.nextRole === 'pharmacist' && (
                      <div className="mt-4">
                        <label className="block text-sm font-semibold mb-1">Số giấy phép (Tùy chọn)</label>
                        <input
                          type="text"
                          placeholder="Ví dụ: CCHN-12345"
                          value={confirm.licenseNumber || ''}
                          onChange={(e) => setConfirm({ ...confirm, licenseNumber: e.target.value })}
                          className="w-full px-3 py-2 rounded-lg border border-outline-variant/40 bg-surface-container-low outline-none focus:border-primary text-sm"
                        />
                        <p className="text-xs text-on-surface-variant mt-1">Để trống sẽ tự động tạo mã PENDING.</p>
                      </div>
                    )}
                  </>
                )}
                {confirm.kind === 'lock' && (
                  <>
                    {confirm.nextActive ? 'Mở khóa' : 'Khóa'} tài khoản <b>{confirm.user.fullName}</b>?
                  </>
                )}
                {confirm.kind === 'delete' && (
                  <>
                    Xóa vĩnh viễn người dùng <b>{confirm.user.fullName}</b>? Tất cả dữ liệu liên quan (đơn hàng, tủ thuốc, chẩn đoán...) sẽ bị xóa và không thể khôi phục.
                  </>
                )}
              </p>
              {actionError && (
                <p className="mt-3 text-error text-sm bg-error-container/40 px-3 py-2 rounded">{actionError}</p>
              )}
            </>
          ) : null
        }
        onCancel={() => {
          setConfirm(null)
          setActionError(null)
        }}
        onConfirm={executeConfirm}
      />
    </AdminPageShell>
  )
}
