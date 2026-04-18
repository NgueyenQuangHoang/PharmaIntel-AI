import type { PropsWithChildren } from 'react'
import { cn } from '@/utils/cn'

type SectionCardProps = PropsWithChildren<{
  className?: string
  title: string
  description: string
}>

export function SectionCard({
  children,
  className,
  title,
  description,
}: SectionCardProps) {
  return (
    <article
      className={cn(
        'rounded-3xl border border-white/10 bg-white/5 p-6 shadow-2xl shadow-black/20 backdrop-blur',
        className,
      )}
    >
      <div className="space-y-2">
        <p className="text-xs font-semibold uppercase tracking-[0.24em] text-cyan-300">
          Starter module
        </p>
        <h2 className="text-2xl font-semibold text-white">{title}</h2>
        <p className="text-sm leading-6 text-slate-300">{description}</p>
      </div>
      <div className="mt-6">{children}</div>
    </article>
  )
}
