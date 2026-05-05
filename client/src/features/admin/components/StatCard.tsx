// =============================================================================
// StatCard - card hien thi 1 chi so cho admin dashboard.
// Cung concept glass-card cua app: backdrop blur, soft border, ambient shadow.
// =============================================================================
import { cn } from '@/utils/cn'

type Props = {
  icon: string
  label: string
  value: string | number
  hint?: string
  tone?: 'primary' | 'secondary' | 'tertiary' | 'neutral'
  className?: string
}

const TONE_CLASSES: Record<NonNullable<Props['tone']>, string> = {
  primary: 'bg-primary-container/40 text-primary',
  secondary: 'bg-secondary-container/40 text-secondary',
  tertiary: 'bg-tertiary-container/40 text-tertiary',
  neutral: 'bg-surface-container-high text-on-surface-variant',
}

export function StatCard({ icon, label, value, hint, tone = 'primary', className }: Props) {
  return (
    <div
      className={cn(
        'rounded-2xl border border-outline-variant/40 bg-surface-container/60 p-5 backdrop-blur ambient-shadow',
        className,
      )}
    >
      <div className="flex items-center gap-3">
        <span
          className={cn(
            'flex h-11 w-11 items-center justify-center rounded-xl',
            TONE_CLASSES[tone],
          )}
        >
          <span className="material-symbols-outlined text-[22px]">{icon}</span>
        </span>
        <p className="text-sm font-medium text-on-surface-variant">{label}</p>
      </div>
      <p className="mt-4 font-headline text-3xl font-bold text-on-surface tracking-tight">
        {value}
      </p>
      {hint && <p className="mt-1 text-xs text-on-surface-variant">{hint}</p>}
    </div>
  )
}
