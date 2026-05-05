// =============================================================================
// DataTable - bang du lieu generic cho admin pages.
// Ho tro:
//   - columns: dinh nghia cot voi accessor function de render cell
//   - rows: data
//   - emptyMessage: hien thi khi rows rong
//   - footer: ReactNode (vi du paging controls)
// =============================================================================
import type { ReactNode } from 'react'

export type DataTableColumn<T> = {
  key: string
  header: string
  cell: (row: T) => ReactNode
  className?: string
  headerClassName?: string
}

type Props<T> = {
  columns: DataTableColumn<T>[]
  rows: T[]
  rowKey: (row: T) => string | number
  loading?: boolean
  emptyMessage?: string
  footer?: ReactNode
}

export function DataTable<T>({
  columns,
  rows,
  rowKey,
  loading,
  emptyMessage = 'Không có dữ liệu',
  footer,
}: Props<T>) {
  return (
    <div className="rounded-2xl border border-outline-variant/40 bg-surface-container/60 backdrop-blur overflow-hidden ambient-shadow">
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead className="bg-surface-container-high/60">
            <tr>
              {columns.map((c) => (
                <th
                  key={c.key}
                  className={`px-4 py-3 text-left font-semibold text-on-surface-variant text-xs uppercase tracking-wider ${c.headerClassName ?? ''}`}
                >
                  {c.header}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {loading && (
              <tr>
                <td colSpan={columns.length} className="px-4 py-10 text-center text-on-surface-variant">
                  <span className="material-symbols-outlined animate-spin text-primary text-3xl">progress_activity</span>
                  <p className="mt-2 text-sm">Đang tải...</p>
                </td>
              </tr>
            )}
            {!loading && rows.length === 0 && (
              <tr>
                <td colSpan={columns.length} className="px-4 py-10 text-center text-on-surface-variant">
                  {emptyMessage}
                </td>
              </tr>
            )}
            {!loading &&
              rows.map((row) => (
                <tr
                  key={rowKey(row)}
                  className="border-t border-outline-variant/30 hover:bg-surface-container-high/40 transition-colors"
                >
                  {columns.map((c) => (
                    <td key={c.key} className={`px-4 py-3 text-on-surface ${c.className ?? ''}`}>
                      {c.cell(row)}
                    </td>
                  ))}
                </tr>
              ))}
          </tbody>
        </table>
      </div>
      {footer && (
        <div className="border-t border-outline-variant/30 px-4 py-3 bg-surface-container-low/40">{footer}</div>
      )}
    </div>
  )
}
