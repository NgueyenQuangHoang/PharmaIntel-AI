// =============================================================================
// AdminMedicationsPage - quản lý sản phẩm (thuốc): list + create + update + delete.
// Dùng medicationsApi (đã extend với create/update/delete) + categoriesApi để
// load category options cho dropdown.
// =============================================================================
import { useEffect, useMemo, useRef, useState } from 'react'
import { AdminPageShell } from '@/features/admin/components/AdminPageShell'
import { DataTable, type DataTableColumn } from '@/features/admin/components/DataTable'
import { ConfirmDialog } from '@/features/admin/components/ConfirmDialog'
import { medicationsApi, type MedicationUpsertRequest } from '@/features/medications/medications-api'
import { categoriesApi } from '@/features/categories/categories-api'
import type { Medication, MedicationListItem } from '@/features/medications/types'
import type { Category } from '@/features/categories/types'
import { formatVnd } from '@/utils/format'

type EditState = { mode: 'create' } | { mode: 'edit'; medication: Medication } | null

export function AdminMedicationsPage() {
  const [items, setItems] = useState<MedicationListItem[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [page, setPage] = useState(1)
  const [pageSize] = useState(20)
  const [search, setSearch] = useState('')
  const [searchInput, setSearchInput] = useState('')
  const [categories, setCategories] = useState<Category[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [edit, setEdit] = useState<EditState>(null)
  const [deleting, setDeleting] = useState<MedicationListItem | null>(null)
  const [busy, setBusy] = useState(false)
  const [actionError, setActionError] = useState<string | null>(null)

  async function loadList() {
    setLoading(true)
    setError(null)
    try {
      const paged = await medicationsApi.list({
        page,
        pageSize,
        q: search || undefined,
        sortBy: 'createdAt',
        sortDesc: true,
      })
      setItems(paged.items)
      setTotalCount(paged.totalCount)
    } catch (err: unknown) {
      const e = err as { response?: { data?: { detail?: string } }; message?: string }
      setError(e.response?.data?.detail ?? e.message ?? 'Không tải được danh sách sản phẩm')
    } finally {
      setLoading(false)
    }
  }

  async function loadCategories() {
    try {
      const paged = await categoriesApi.list({ page: 1, pageSize: 200, isActive: true })
      setCategories(paged.items)
    } catch {
      /* ignore */
    }
  }

  useEffect(() => {
    loadCategories()
  }, [])

  useEffect(() => {
    loadList()
  }, [page, search])

  useEffect(() => {
    const t = setTimeout(() => {
      const q = searchInput.trim()
      if (search !== q) {
        setSearch(q)
        setPage(1)
      }
    }, 300)
    return () => clearTimeout(t)
  }, [searchInput, search])

  async function executeDelete() {
    if (!deleting) return
    setBusy(true)
    setActionError(null)
    try {
      await medicationsApi.delete(deleting.id)
      setDeleting(null)
      await loadList()
    } catch (err: unknown) {
      const e = err as { response?: { data?: { detail?: string; title?: string } } }
      setActionError(e.response?.data?.detail ?? e.response?.data?.title ?? 'Lỗi xóa sản phẩm')
    } finally {
      setBusy(false)
    }
  }

  const columns: DataTableColumn<MedicationListItem>[] = [
    {
      key: 'product',
      header: 'Sản phẩm',
      cell: (m) => (
        <div className="flex items-center gap-3">
          {m.imageUrl ? (
            <img src={m.imageUrl} alt="" className="w-10 h-10 rounded-lg object-cover" />
          ) : (
            <span className="w-10 h-10 rounded-lg bg-surface-container-high flex items-center justify-center">
              <span className="material-symbols-outlined text-on-surface-variant">medication</span>
            </span>
          )}
          <div className="min-w-0">
            <p className="font-semibold text-on-surface truncate">{m.name}</p>
            <p className="text-xs text-on-surface-variant truncate">
              {m.sku} · {m.categoryName}
            </p>
          </div>
        </div>
      ),
    },
    {
      key: 'price',
      header: 'Giá',
      cell: (m) => (
        <div>
          <p className="text-sm font-semibold text-on-surface">{formatVnd(m.finalPrice)}</p>
          {m.discountPercent > 0 && (
            <p className="text-xs text-on-surface-variant line-through">{formatVnd(m.price)}</p>
          )}
        </div>
      ),
    },
    {
      key: 'stock',
      header: 'Tồn kho',
      cell: (m) => (
        <span className={`text-sm ${m.stockQuantity === 0 ? 'text-error font-semibold' : ''}`}>
          {m.stockQuantity}
        </span>
      ),
    },
    {
      key: 'flags',
      header: 'Nhãn',
      cell: (m) => (
        <div className="flex flex-wrap gap-1">
          {m.isFeatured && (
            <span className="px-1.5 py-0.5 rounded text-[10px] font-semibold bg-tertiary-container/60 text-tertiary">
              NỔI BẬT
            </span>
          )}
          {m.isBestSeller && (
            <span className="px-1.5 py-0.5 rounded text-[10px] font-semibold bg-secondary-container/60 text-secondary">
              BÁN CHẠY
            </span>
          )}
          {m.isPrescriptionRequired && (
            <span className="px-1.5 py-0.5 rounded text-[10px] font-semibold bg-error-container/60 text-error">
              KÊ ĐƠN
            </span>
          )}
          {!m.isActive && (
            <span className="px-1.5 py-0.5 rounded text-[10px] font-semibold bg-surface-container-high text-on-surface-variant">
              ẨN
            </span>
          )}
        </div>
      ),
    },
    {
      key: 'actions',
      header: '',
      headerClassName: 'text-right',
      className: 'text-right',
      cell: (m) => (
        <div className="flex items-center justify-end gap-1">
          <button
            type="button"
            onClick={async () => {
              try {
                const full = await medicationsApi.getById(m.id)
                setEdit({ mode: 'edit', medication: full })
              } catch {
                /* ignore */
              }
            }}
            className="p-1.5 rounded-lg text-on-surface-variant hover:bg-surface-container-high"
            title="Sửa"
          >
            <span className="material-symbols-outlined text-[18px]">edit</span>
          </button>
          <button
            type="button"
            onClick={() => setDeleting(m)}
            className="p-1.5 rounded-lg text-error hover:bg-error-container/40"
            title="Xóa"
          >
            <span className="material-symbols-outlined text-[18px]">delete</span>
          </button>
        </div>
      ),
    },
  ]

  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize))

  return (
    <AdminPageShell
      title="Quản lý sản phẩm"
      description={`Tổng: ${totalCount.toLocaleString('vi-VN')} sản phẩm`}
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
      <div className="mb-4 flex items-center gap-2">
        <div className="flex flex-1 items-center gap-2 rounded-xl border border-outline-variant/40 bg-surface-container-low px-3 py-2">
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
            onKeyDown={(e) => {
              if (e.key === 'Enter') {
                setPage(1)
                setSearch(searchInput.trim())
              }
            }}
            placeholder="Tìm tên, SKU, hãng sản xuất..."
            className="flex-1 bg-transparent outline-none text-sm text-on-surface placeholder:text-on-surface-variant"
          />
        </div>
        <button
          type="button"
          onClick={() => {
            setPage(1)
            setSearch(searchInput.trim())
          }}
          className="rounded-xl bg-primary text-on-primary px-4 py-2 text-sm font-semibold hover:bg-primary/90"
        >
          Tìm
        </button>
      </div>

      {error && (
        <p className="mb-3 text-sm text-error bg-error-container/40 px-4 py-2 rounded-lg">{error}</p>
      )}

      <DataTable<MedicationListItem>
        columns={columns}
        rows={items}
        rowKey={(m) => m.id}
        loading={loading}
        emptyMessage="Không tìm thấy sản phẩm"
        footer={
          <div className="flex items-center justify-between text-sm text-on-surface-variant">
            <span>
              Trang {page} / {totalPages}
            </span>
            <div className="flex gap-1">
              <button
                type="button"
                disabled={page <= 1}
                onClick={() => setPage((p) => p - 1)}
                className="px-3 py-1.5 rounded-lg border border-outline-variant/40 disabled:opacity-30 hover:bg-surface-container-high"
              >
                Trước
              </button>
              <button
                type="button"
                disabled={page >= totalPages}
                onClick={() => setPage((p) => p + 1)}
                className="px-3 py-1.5 rounded-lg border border-outline-variant/40 disabled:opacity-30 hover:bg-surface-container-high"
              >
                Sau
              </button>
            </div>
          </div>
        }
      />

      {edit && (
        <MedicationFormModal
          categories={categories}
          initial={edit.mode === 'edit' ? edit.medication : null}
          onClose={() => setEdit(null)}
          onSaved={async () => {
            setEdit(null)
            await loadList()
          }}
        />
      )}

      <ConfirmDialog
        open={!!deleting}
        busy={busy}
        variant="danger"
        title="Xóa sản phẩm"
        message={
          deleting ? (
            <>
              <p>
                Xóa sản phẩm <b>{deleting.name}</b>? Hành động này không thực hiện được nếu sản phẩm
                đang có trong giỏ hàng hoặc đơn hàng.
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

function MedicationFormModal({
  categories,
  initial,
  onClose,
  onSaved,
}: {
  categories: Category[]
  initial: Medication | null
  onClose: () => void
  onSaved: () => void
}) {
  const isEdit = !!initial
  const defaultCategoryId = useMemo(() => initial?.categoryId ?? categories[0]?.id ?? 0, [initial, categories])
  const [form, setForm] = useState<MedicationUpsertRequest>({
    sku: initial?.sku ?? '',
    name: initial?.name ?? '',
    genericName: initial?.genericName ?? null,
    manufacturer: initial?.manufacturer ?? null,
    registrationNumber: initial?.registrationNumber ?? null,
    description: initial?.description ?? null,
    dosage: initial?.dosage ?? null,
    packaging: initial?.packaging ?? null,
    price: initial?.price ?? 0,
    discountPercent: initial?.discountPercent ?? 0,
    categoryId: defaultCategoryId,
    usageInstructions: initial?.usageInstructions ?? null,
    benefits: initial?.benefits ?? null,
    activeIngredients: initial?.activeIngredients ?? null,
    contraindications: initial?.contraindications ?? null,
    sideEffects: initial?.sideEffects ?? null,
    storageInstructions: initial?.storageInstructions ?? null,
    imageUrl: initial?.imageUrl ?? null,
    isFeatured: initial?.isFeatured ?? false,
    isBestSeller: initial?.isBestSeller ?? false,
    isPrescriptionRequired: initial?.isPrescriptionRequired ?? false,
    stockQuantity: initial?.stockQuantity ?? 0,
    isActive: initial?.isActive ?? true,
  })
  const [busy, setBusy] = useState(false)
  const [err, setErr] = useState<string | null>(null)
  const [uploading, setUploading] = useState(false)
  const [uploadErr, setUploadErr] = useState<string | null>(null)
  const fileInputRef = useRef<HTMLInputElement>(null)

  function set<K extends keyof MedicationUpsertRequest>(k: K, v: MedicationUpsertRequest[K]) {
    setForm((f) => ({ ...f, [k]: v }))
  }
  function setStr<K extends keyof MedicationUpsertRequest>(k: K) {
    return (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
      const v = e.target.value
      set(k, (v === '' ? null : v) as MedicationUpsertRequest[K])
    }
  }

  async function submit(e: React.FormEvent) {
    e.preventDefault()
    setBusy(true)
    setErr(null)
    try {
      if (isEdit && initial) {
        await medicationsApi.update(initial.id, form)
      } else {
        await medicationsApi.create(form)
      }
      onSaved()
    } catch (error: unknown) {
      const ex = error as { response?: { data?: { detail?: string; title?: string } } }
      setErr(ex.response?.data?.detail ?? ex.response?.data?.title ?? 'Lưu không thành công')
    } finally {
      setBusy(false)
    }
  }

  async function handleFileChange(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0]
    if (!file) return
    setUploading(true)
    setUploadErr(null)
    try {
      const url = await medicationsApi.uploadImage(file)
      set('imageUrl', url)
    } catch (error: unknown) {
      const ex = error as { response?: { data?: { detail?: string } }; message?: string }
      setUploadErr(ex.response?.data?.detail ?? ex.message ?? 'Upload thất bại')
    } finally {
      setUploading(false)
      // Reset input để có thể chọn lại cùng file
      if (fileInputRef.current) fileInputRef.current.value = ''
    }
  }

  return (
    <div className="fixed inset-0 z-[100] flex items-center justify-center p-4">
      <div className="absolute inset-0 bg-black/40 backdrop-blur-sm" onClick={onClose} />
      <form
        onSubmit={submit}
        className="relative w-full max-w-2xl rounded-2xl bg-surface-container border border-outline-variant/40 p-6 shadow-2xl max-h-[90vh] overflow-y-auto"
      >
        <h3 className="font-headline text-lg font-bold text-on-surface mb-4">
          {isEdit ? 'Sửa sản phẩm' : 'Tạo sản phẩm mới'}
        </h3>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
          <Field label="SKU *">
            <Input value={form.sku} onChange={(e) => set('sku', e.target.value)} required />
          </Field>
          <Field label="Tên *">
            <Input value={form.name} onChange={(e) => set('name', e.target.value)} required />
          </Field>
          <Field label="Tên hoạt chất (generic)">
            <Input value={form.genericName ?? ''} onChange={setStr('genericName')} />
          </Field>
          <Field label="Hãng sản xuất">
            <Input value={form.manufacturer ?? ''} onChange={setStr('manufacturer')} />
          </Field>
          <Field label="Số đăng ký">
            <Input value={form.registrationNumber ?? ''} onChange={setStr('registrationNumber')} />
          </Field>
          <Field label="Danh mục *">
            <select
              required
              value={form.categoryId}
              onChange={(e) => set('categoryId', Number(e.target.value))}
              className="w-full rounded-lg border border-outline-variant/40 bg-surface-container-low px-3 py-2 text-sm text-on-surface outline-none focus:border-primary"
            >
              {categories.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.name}
                </option>
              ))}
            </select>
          </Field>
          <Field label="Giá (VND) *">
            <Input
              type="number"
              min={0}
              required
              value={form.price}
              onChange={(e) => set('price', Number(e.target.value))}
            />
          </Field>
          <Field label="% Giảm giá">
            <Input
              type="number"
              min={0}
              max={100}
              value={form.discountPercent}
              onChange={(e) => set('discountPercent', Number(e.target.value))}
            />
          </Field>
          <Field label="Tồn kho">
            <Input
              type="number"
              min={0}
              value={form.stockQuantity}
              onChange={(e) => set('stockQuantity', Number(e.target.value))}
            />
          </Field>
          <Field label="Quy cách (VD: hộp 10 viên)">
            <Input value={form.packaging ?? ''} onChange={setStr('packaging')} />
          </Field>
          <Field label="Liều lượng">
            <Input value={form.dosage ?? ''} onChange={setStr('dosage')} />
          </Field>
          <Field label="URL ảnh">
            <div className="space-y-2">
              {/* Preview ảnh hiện tại */}
              {form.imageUrl && (
                <div className="relative w-full h-32 rounded-lg overflow-hidden border border-outline-variant/40 bg-surface-container-low">
                  <img
                    src={form.imageUrl}
                    alt="preview"
                    className="w-full h-full object-contain"
                    onError={(e) => {
                      ;(e.target as HTMLImageElement).style.display = 'none'
                    }}
                  />
                  <button
                    type="button"
                    onClick={() => set('imageUrl', null)}
                    className="absolute top-1 right-1 p-1 rounded-full bg-error text-white hover:bg-error/80"
                    title="Xóa ảnh"
                  >
                    <span className="material-symbols-outlined text-[14px]">close</span>
                  </button>
                </div>
              )}
              {/* Upload button */}
              <input
                ref={fileInputRef}
                type="file"
                accept="image/jpeg,image/png,image/webp,image/gif"
                className="hidden"
                onChange={handleFileChange}
              />
              <button
                type="button"
                disabled={uploading || busy}
                onClick={() => fileInputRef.current?.click()}
                className="w-full flex items-center justify-center gap-2 rounded-lg border border-dashed border-outline-variant/60 bg-surface-container-low px-3 py-2 text-sm text-on-surface-variant hover:border-primary hover:text-primary transition-colors disabled:opacity-50"
              >
                {uploading ? (
                  <>
                    <span className="material-symbols-outlined text-[18px] animate-spin">progress_activity</span>
                    Đang upload...
                  </>
                ) : (
                  <>
                    <span className="material-symbols-outlined text-[18px]">upload</span>
                    {form.imageUrl ? 'Đổi ảnh' : 'Chọn ảnh (JPG, PNG, WebP, GIF · max 5MB)'}
                  </>
                )}
              </button>
              {uploadErr && (
                <p className="text-xs text-error">{uploadErr}</p>
              )}
              {/* Fallback: nhập URL thủ công */}
              <details className="group">
                <summary className="text-xs text-on-surface-variant cursor-pointer hover:text-primary select-none">
                  hoặc nhập URL ảnh trực tiếp
                </summary>
                <Input
                  className="mt-1"
                  value={form.imageUrl ?? ''}
                  onChange={setStr('imageUrl')}
                  placeholder="https://..."
                />
              </details>
            </div>
          </Field>
        </div>
        <div className="mt-3 grid grid-cols-1 sm:grid-cols-2 gap-3">
          <Field label="Mô tả ngắn">
            <Textarea rows={2} value={form.description ?? ''} onChange={setStr('description')} />
          </Field>
          <Field label="Công dụng">
            <Textarea rows={2} value={form.benefits ?? ''} onChange={setStr('benefits')} />
          </Field>
          <Field label="Hoạt chất">
            <Textarea rows={2} value={form.activeIngredients ?? ''} onChange={setStr('activeIngredients')} />
          </Field>
          <Field label="Chống chỉ định">
            <Textarea rows={2} value={form.contraindications ?? ''} onChange={setStr('contraindications')} />
          </Field>
          <Field label="Tác dụng phụ">
            <Textarea rows={2} value={form.sideEffects ?? ''} onChange={setStr('sideEffects')} />
          </Field>
          <Field label="Bảo quản">
            <Textarea rows={2} value={form.storageInstructions ?? ''} onChange={setStr('storageInstructions')} />
          </Field>
          <Field label="Hướng dẫn sử dụng">
            <Textarea rows={2} value={form.usageInstructions ?? ''} onChange={setStr('usageInstructions')} />
          </Field>
        </div>
        <div className="mt-3 flex flex-wrap gap-4">
          <Toggle checked={form.isActive} onChange={(v) => set('isActive', v)} label="Hoạt động" />
          <Toggle checked={form.isFeatured} onChange={(v) => set('isFeatured', v)} label="Nổi bật" />
          <Toggle checked={form.isBestSeller} onChange={(v) => set('isBestSeller', v)} label="Bán chạy" />
          <Toggle
            checked={form.isPrescriptionRequired}
            onChange={(v) => set('isPrescriptionRequired', v)}
            label="Yêu cầu kê đơn"
          />
        </div>
        {err && <p className="mt-3 text-error text-sm bg-error-container/40 px-3 py-2 rounded">{err}</p>}
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

function Input(props: React.InputHTMLAttributes<HTMLInputElement>) {
  return (
    <input
      {...props}
      className="w-full rounded-lg border border-outline-variant/40 bg-surface-container-low px-3 py-2 text-sm text-on-surface outline-none focus:border-primary"
    />
  )
}

function Textarea(props: React.TextareaHTMLAttributes<HTMLTextAreaElement>) {
  return (
    <textarea
      {...props}
      className="w-full rounded-lg border border-outline-variant/40 bg-surface-container-low px-3 py-2 text-sm text-on-surface outline-none focus:border-primary resize-none"
    />
  )
}

function Toggle({ checked, onChange, label }: { checked: boolean; onChange: (v: boolean) => void; label: string }) {
  return (
    <label className="inline-flex items-center gap-2 cursor-pointer">
      <input
        type="checkbox"
        checked={checked}
        onChange={(e) => onChange(e.target.checked)}
        className="w-4 h-4 accent-primary"
      />
      <span className="text-sm text-on-surface">{label}</span>
    </label>
  )
}
