import { useEffect, useRef, useState } from 'react'
import { medicationsApi } from '@/features/medications/medications-api'
import type { MedicationListItem } from '@/features/medications/types'
import { useDebouncedValue } from '@/hooks/useDebouncedValue'

export type MedicationComboboxValue = {
  medicationId: number | null
  medicationName: string
}

type Props = {
  value: MedicationComboboxValue
  onChange: (next: MedicationComboboxValue) => void
  placeholder?: string
  disabled?: boolean
}

export function MedicationCombobox({ value, onChange, placeholder, disabled }: Props) {
  const [open, setOpen] = useState(false)
  const [suggestions, setSuggestions] = useState<MedicationListItem[]>([])
  const [loading, setLoading] = useState(false)
  const [searched, setSearched] = useState(false)
  const wrapperRef = useRef<HTMLDivElement>(null)

  const debouncedName = useDebouncedValue(value.medicationName, 250)

  useEffect(() => {
    const q = debouncedName.trim()
    if (q.length < 2) {
      setSuggestions([])
      setSearched(false)
      return
    }
    if (value.medicationId !== null) {
      // Vua chon xong - debounced van bang ten da chon, khong can search lai.
      return
    }
    let cancelled = false
    setLoading(true)
    medicationsApi
      .list({ q, pageSize: 8, isActive: true })
      .then((res) => {
        if (cancelled) return
        setSuggestions(res.items)
        setSearched(true)
      })
      .catch(() => {
        if (!cancelled) setSuggestions([])
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })
    return () => {
      cancelled = true
    }
  }, [debouncedName, value.medicationId])

  // Click ra ngoai -> dong dropdown.
  useEffect(() => {
    function handleClick(e: MouseEvent) {
      if (wrapperRef.current && !wrapperRef.current.contains(e.target as Node)) {
        setOpen(false)
      }
    }
    document.addEventListener('mousedown', handleClick)
    return () => document.removeEventListener('mousedown', handleClick)
  }, [])

  function pickSuggestion(m: MedicationListItem) {
    onChange({ medicationId: m.id, medicationName: m.name })
    setOpen(false)
  }

  return (
    <div ref={wrapperRef} className="relative">
      <div className="flex items-center gap-2">
        <div className="relative flex-1">
          <input
            type="text"
            value={value.medicationName ?? ''}
            onChange={(e) => {
              onChange({ medicationId: null, medicationName: e.target.value })
              setOpen(true)
            }}
            onFocus={() => setOpen(true)}
            onKeyDown={(e) => {
              if (e.key === 'Escape') setOpen(false)
            }}
            placeholder={placeholder}
            disabled={disabled}
            className="w-full rounded-lg bg-surface px-3 py-2 text-sm border border-outline-variant/30 focus:outline-none focus:ring-2 focus:ring-primary pr-8"
          />
          {loading && (
            <span className="absolute right-2 top-1/2 -translate-y-1/2 material-symbols-outlined text-on-surface-variant text-base animate-spin">
              progress_activity
            </span>
          )}
        </div>
        <span
          className={`text-[10px] px-2 py-1 rounded font-semibold shrink-0 ${
            value.medicationId
              ? 'bg-primary-container text-on-primary-container'
              : 'bg-surface-container-high text-on-surface-variant'
          }`}
          title={value.medicationId ? `medicationId=${value.medicationId}` : 'Lưu dạng tự do, không link tủ thuốc'}
        >
          {value.medicationId ? 'Đã link tủ thuốc' : 'Tự do'}
        </span>
      </div>

      {open && value.medicationName.trim().length >= 2 && (
        <div className="absolute z-30 mt-1 w-full bg-surface rounded-xl shadow-lg border border-outline-variant/30 max-h-72 overflow-y-auto">
          {suggestions.length > 0 ? (
            <ul className="py-1">
              {suggestions.map((m) => (
                <li key={m.id}>
                  <button
                    type="button"
                    onClick={() => pickSuggestion(m)}
                    className="w-full text-left px-3 py-2 hover:bg-surface-container-low transition-colors"
                  >
                    <div className="flex items-center justify-between gap-2">
                      <div className="font-semibold text-sm">{m.name}</div>
                      {m.isPrescriptionRequired && (
                        <span className="text-[10px] px-1.5 py-0.5 rounded bg-error-container text-on-error-container font-semibold shrink-0">
                          Cần đơn
                        </span>
                      )}
                    </div>
                    <div className="text-xs text-on-surface-variant mt-0.5">
                      {[m.manufacturer, m.genericName].filter(Boolean).join(' • ') || '—'}
                    </div>
                  </button>
                </li>
              ))}
            </ul>
          ) : searched && !loading ? (
            <div className="px-3 py-3 text-xs text-on-surface-variant italic">
              Không có trong tủ thuốc — sẽ lưu dạng tự do khi nhấn Thêm.
            </div>
          ) : null}
        </div>
      )}
    </div>
  )
}
