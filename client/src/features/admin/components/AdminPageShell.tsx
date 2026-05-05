// =============================================================================
// AdminPageShell - layout cua khu vuc admin: sidebar + main content.
// Su dung ben trong MainLayout (Header + Footer cua app van hien thi).
// =============================================================================
import type { ReactNode } from 'react'
import { AdminSidebar } from './AdminSidebar'

type Props = {
  title: string
  description?: string
  actions?: ReactNode
  children: ReactNode
}

export function AdminPageShell({ title, description, actions, children }: Props) {
  return (
    <div className="mx-auto max-w-7xl px-4 md:px-8 py-6">
      <div className="md:flex md:gap-8">
        <AdminSidebar />
        <main className="flex-1 min-w-0">
          <header className="flex flex-col gap-3 md:flex-row md:items-end md:justify-between mb-6">
            <div>
              <h1 className="font-headline text-2xl md:text-3xl font-bold text-on-surface tracking-tight">
                {title}
              </h1>
              {description && (
                <p className="mt-1 text-sm text-on-surface-variant">{description}</p>
              )}
            </div>
            {actions && <div className="flex items-center gap-2">{actions}</div>}
          </header>
          {children}
        </main>
      </div>
    </div>
  )
}
