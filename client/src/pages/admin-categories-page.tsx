// =============================================================================
// AdminCategoriesPage - quản lý danh mục: list + create + update + delete.
// Tải lại cây danh mục mới khi có thay đổi để hiển thị parent dropdown.
// =============================================================================
import { useEffect, useState } from 'react'
import { AdminPageShell } from '@/features/admin/components/AdminPageShell'
import { DataTable, type DataTableColumn } from '@/features/admin/components/DataTable'
import { ConfirmDialog } from '@/features/admin/components/ConfirmDialog'
import { categoriesApi, type CategoryUpsertRequest } from '@/features/categories/categories-api'
import type { Category, CategoryListQuery } from '@/features/categories/types'

type EditState =
  | { mode: 'create' }
  | { mode: 'edit'; category: Category }
  | null

export function AdminCategoriesPage() {
  const [categories, setCategories] = useState<Category[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [filter, setFilter] = useState<CategoryListQuery>({ page: 1, pageSize: 20 })
  const [searchInput, setSearchInput] = useState('')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [edit, setEdit] = useState<EditState>(null)
  const [deleting, setDeleting] = useState<Category | null>(null)
  const [busy, setBusy] = useState(false)
  const [actionError, setActionError] = useState<string | null>(null)

  async function load() {
    setLoading(true)
    setError(null)
    try {
      const paged = await categoriesApi.list(filter)
      setCategories(paged.items)
      setTotalCount(paged.totalCount)
    } catch (err: unknown) {
      const e = err as { message?: string; response?: { data?: { detail?: string } } }
      setError(e.response?.data?.detail ?? e.message ?? 'Không tải được danh mục')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    load()
  }, [filter])

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

  async function executeDelete() {
    if (!deleting) return
    setBusy(true)
    setActionError(null)
    try {
      await categoriesApi.delete(deleting.id)
      setDeleting(null)
      await load()
    } catch (err: unknown) {
      const e = err as { response?: { data?: { detail?: string; title?: string } } }
      setActionError(e.response?.data?.detail ?? e.response?.data?.title ?? 'Lỗi xóa danh mục')
    } finally {
      setBusy(false)
    }
  }

  const columns: DataTableColumn<Category>[] = [
    {
      key: 'name',
      header: 'Danh mục',
      cell: (c) => (
        <div>
          <p className="font-semibold text-on-surface">{c.name}</p>
          <p className="text-xs text-on-surface-variant">/{c.slug}</p>
        </div>
      ),
    },
    {
      key: 'count',
      header: 'Số sản phẩm',
      cell: (c) => <span className="text-sm">{c.medicationCount}</span>,
    },
    {
      key: 'order',
      header: 'Thứ tự',
      cell: (c) => <span className="text-sm">{c.displayOrder}</span>,
    },
    {
      key: 'active',
      header: 'Trạng thái',
      cell: (c) => (
        <span
          className={`whitespace-nowrap px-2 py-0.5 rounded-full text-xs font-semibold ${
            c.isActive ? 'bg-primary-container/60 text-primary' : 'bg-surface-container-high text-on-surface-variant'
          }`}
        >
          {c.isActive ? 'Hoạt động' : 'Ẩn'}
        </span>
      ),
    },
    {
      key: 'actions',
      header: '',
      headerClassName: 'text-right',
      className: 'text-right',
      cell: (c) => (
        <div className="flex items-center justify-end gap-1">
          <button
            type="button"
            onClick={() => setEdit({ mode: 'edit', category: c })}
            className="p-1.5 rounded-lg text-on-surface-variant hover:bg-surface-container-high"
            title="Sửa"
          >
            <span className="material-symbols-outlined text-[18px]">edit</span>
          </button>
          <button
            type="button"
            onClick={() => setDeleting(c)}
            className="p-1.5 rounded-lg text-error hover:bg-error-container/40"
            title="Xóa"
          >
            <span className="material-symbols-outlined text-[18px]">delete</span>
          </button>
        </div>
      ),
    },
  ]

  const totalPages = Math.max(1, Math.ceil(totalCount / (filter.pageSize ?? 20)))

  return (
    <AdminPageShell
      title="Quản lý danh mục"
      description={`Tổng: ${totalCount.toLocaleString('vi-VN')} danh mục`}
      actions={
        <button
          type="button"
          onClick={() => setEdit({ mode: 'create' })}
          className="inline-flex items-center gap-2 rounded-xl bg-primary text-on-primary px-4 py-2 text-sm font-semibold hover:bg-primary/90"
        >
          <span className="material-symbols-outlined text-[18px]">add</span>
          Tạo mới
        </button>
      }
    >
      <div className="mb-4 flex flex-col sm:flex-row gap-2">
        <div className="flex flex-1 items-center gap-2 rounded-xl border border-outline-variant/40 bg-surface-container-low px-3 py-2 input-ghost-border">
          {loading ? (
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
            placeholder="Tìm tên, slug danh mục..."
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
      </div>

      {error && (
        <p className="mb-3 text-sm text-error bg-error-container/40 px-4 py-2 rounded-lg">{error}</p>
      )}

      <DataTable<Category>
        columns={columns}
        rows={categories}
        rowKey={(c) => c.id}
        loading={loading}
        emptyMessage="Chưa có danh mục nào"
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

      {edit && (
        <CategoryFormModal
          initial={edit.mode === 'edit' ? edit.category : null}
          onClose={() => setEdit(null)}
          onSaved={async () => {
            setEdit(null)
            await load()
          }}
        />
      )}

      <ConfirmDialog
        open={!!deleting}
        busy={busy}
        variant="danger"
        title="Xóa danh mục"
        message={
          deleting ? (
            <>
              <p>
                Xóa danh mục <b>{deleting.name}</b>? Hành động này không thực hiện được nếu danh mục
                có danh mục con hoặc đang chứa sản phẩm.
              </p>
              {actionError && (
                <p className="mt-3 text-error text-sm bg-error-container/40 px-3 py-2 rounded">{actionError}</p>
              )}
            </>
          ) : null
        }
        onCancel={() => {
          setDeleting(null)
          setActionError(null)
        }}
        onConfirm={executeDelete}
      />
    </AdminPageShell>
  )
}

// === Form modal ===
function CategoryFormModal({
  initial,
  onClose,
  onSaved,
}: {
  initial: Category | null
  onClose: () => void
  onSaved: () => void
}) {
  const isEdit = !!initial
  const [form, setForm] = useState<CategoryUpsertRequest>({
    name: initial?.name ?? '',
    slug: initial?.slug ?? '',
    icon: initial?.icon ?? '',
    displayOrder: initial?.displayOrder ?? 0,
    isActive: initial?.isActive ?? true,
  })
  const [busy, setBusy] = useState(false)
  const [err, setErr] = useState<string | null>(null)

  async function submit(e: React.FormEvent) {
    e.preventDefault()
    setBusy(true)
    setErr(null)
    try {
      const payload: CategoryUpsertRequest = {
        ...form,
        slug: form.slug?.trim() || undefined,
        icon: form.icon?.trim() || null,
      }
      if (isEdit && initial) {
        await categoriesApi.update(initial.id, payload)
      } else {
        await categoriesApi.create(payload)
      }
      onSaved()
    } catch (error: unknown) {
      const e = error as { response?: { data?: { detail?: string; title?: string } } }
      setErr(e.response?.data?.detail ?? e.response?.data?.title ?? 'Lưu không thành công')
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="fixed inset-0 z-[100] flex items-center justify-center p-4">
      <div className="absolute inset-0 bg-black/40 backdrop-blur-sm" onClick={onClose} />
      <form
        onSubmit={submit}
        className="relative w-full max-w-md rounded-2xl bg-surface-container border border-outline-variant/40 p-6 shadow-2xl max-h-[90vh] overflow-y-auto"
      >
        <h3 className="font-headline text-lg font-bold text-on-surface mb-4">
          {isEdit ? 'Sửa danh mục' : 'Tạo danh mục mới'}
        </h3>
        <div className="space-y-3">
          <Field label="Tên *">
            <input
              required
              type="text"
              value={form.name}
              onChange={(e) => setForm({ ...form, name: e.target.value })}
              className="w-full rounded-lg border border-outline-variant/40 bg-surface-container-low px-3 py-2 text-sm text-on-surface outline-none focus:border-primary"
            />
          </Field>
          <Field label="Slug (để trống để tự sinh)">
            <input
              type="text"
              value={form.slug ?? ''}
              onChange={(e) => setForm({ ...form, slug: e.target.value })}
              className="w-full rounded-lg border border-outline-variant/40 bg-surface-container-low px-3 py-2 text-sm text-on-surface outline-none focus:border-primary"
            />
          </Field>
          <Field label="Icon (tên material-symbols)">
            <input
              type="text"
              value={form.icon ?? ''}
              onChange={(e) => setForm({ ...form, icon: e.target.value })}
              placeholder="VD: medication"
              className="w-full rounded-lg border border-outline-variant/40 bg-surface-container-low px-3 py-2 text-sm text-on-surface outline-none focus:border-primary"
            />
          </Field>
          <div className="grid grid-cols-2 gap-3">
            <Field label="Thứ tự">
              <input
                type="number"
                value={form.displayOrder}
                onChange={(e) => setForm({ ...form, displayOrder: Number(e.target.value) })}
                className="w-full rounded-lg border border-outline-variant/40 bg-surface-container-low px-3 py-2 text-sm text-on-surface outline-none focus:border-primary"
              />
            </Field>
            <Field label="Trạng thái">
              <select
                value={String(form.isActive)}
                onChange={(e) => setForm({ ...form, isActive: e.target.value === 'true' })}
                className="w-full rounded-lg border border-outline-variant/40 bg-surface-container-low px-3 py-2 text-sm text-on-surface outline-none focus:border-primary"
              >
                <option value="true">Hoạt động</option>
                <option value="false">Ẩn</option>
              </select>
            </Field>
          </div>
        </div>
        {err && (
          <p className="mt-3 text-error text-sm bg-error-container/40 px-3 py-2 rounded">{err}</p>
        )}
        <div className="mt-6 flex justify-end gap-2">
          <button
            type="button"
            onClick={onClose}
            disabled={busy}
            className="px-4 py-2 rounded-lg text-sm font-semibold text-on-surface-variant hover:bg-surface-container-high"
          >
            Hủy
          </button>
          <button
            type="submit"
            disabled={busy}
            className="px-4 py-2 rounded-lg bg-primary text-on-primary text-sm font-semibold hover:bg-primary/90 disabled:opacity-50"
          >
            {busy ? 'Đang lưu...' : 'Lưu'}
          </button>
        </div>
      </form>
    </div>
  )
}

function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <label className="block">
      <span className="text-xs font-semibold text-on-surface-variant block mb-1">{label}</span>
      {children}
    </label>
  )
}
