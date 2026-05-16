// =============================================================================
// AdminSidebar - sidebar dieu huong cho khu vuc admin.
// Desktop: cot doc 240px ben trai. Mobile: tab strip ngang scrollable.
// Cung concept: glass-panel, MD3 tokens, material-symbols-outlined.
// =============================================================================
import { NavLink } from 'react-router-dom'

const NAV_ITEMS = [
  { to: '/admin', icon: 'dashboard', label: 'Tổng quan', end: true },
  { to: '/admin/users', icon: 'group', label: 'Người dùng' },
  { to: '/admin/pharmacists', icon: 'local_pharmacy', label: 'Dược sĩ' },
  { to: '/admin/categories', icon: 'category', label: 'Danh mục' },
  { to: '/admin/medications', icon: 'medication', label: 'Sản phẩm' },
  { to: '/admin/orders', icon: 'receipt_long', label: 'Đơn hàng' },
  { to: '/admin/banks', icon: 'account_balance', label: 'Ngân hàng' },
  { to: '/admin/rag/dashboard', icon: 'monitoring', label: 'RAG Dashboard' },
  { to: '/admin/rag/knowledge', icon: 'library_books', label: 'Tri thức RAG' },
  { to: '/admin/rag/feedback', icon: 'thumbs_up_down', label: 'Feedback AI' },
]

export function AdminSidebar() {
  return (
    <aside className="md:w-60 md:shrink-0">
      {/* Desktop: vertical sidebar */}
      <nav className="hidden md:flex md:flex-col gap-1 sticky top-24 rounded-2xl border border-outline-variant/40 bg-surface-container/60 p-3 backdrop-blur">
        <p className="px-3 pt-2 pb-3 text-xs font-semibold uppercase tracking-[0.2em] text-on-surface-variant">
          Quản trị
        </p>
        {NAV_ITEMS.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            end={item.end}
            className={({ isActive }) =>
              `flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-semibold transition-colors ${
                isActive
                  ? 'bg-primary-container/40 text-primary'
                  : 'text-on-surface-variant hover:bg-surface-container-high hover:text-on-surface'
              }`
            }
          >
            <span className="material-symbols-outlined text-[20px]">{item.icon}</span>
            <span>{item.label}</span>
          </NavLink>
        ))}
      </nav>

      {/* Mobile: horizontal tab strip */}
      <nav className="md:hidden flex gap-2 overflow-x-auto px-1 py-2 -mx-1 mb-4 sticky top-20 z-10 bg-surface/90 backdrop-blur">
        {NAV_ITEMS.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            end={item.end}
            className={({ isActive }) =>
              `flex items-center gap-2 rounded-full px-4 py-2 text-sm font-semibold whitespace-nowrap transition-colors border ${
                isActive
                  ? 'bg-primary-container/40 text-primary border-primary/30'
                  : 'text-on-surface-variant border-outline-variant/40 hover:text-on-surface'
              }`
            }
          >
            <span className="material-symbols-outlined text-[18px]">{item.icon}</span>
            <span>{item.label}</span>
          </NavLink>
        ))}
      </nav>
    </aside>
  )
}
