// =============================================================================
// Mirror logic parse Frequency -> giờ nhắc thuốc cua backend
// (PharmacistPrescriptionVerificationService.ParseDailySlots).
// Dung de hien preview o form duoc si truoc khi verify - bao dam UI va backend
// sinh cung mot bo gio. Neu doi backend, sua file nay theo.
// =============================================================================

// Slot mac dinh theo so lan/ngay. Index 0 khong dung.
const DEFAULT_DAILY_SLOTS: ReadonlyArray<readonly string[]> = [
  [],
  ['08:00'],
  ['08:00', '20:00'],
  ['08:00', '13:00', '20:00'],
  ['07:00', '12:00', '17:00', '22:00'],
]

const DOSES_PER_DAY_REGEX = /(\d+)\s*l(?:ầ|a)n/i

export type ReminderPreview = {
  slots: readonly string[]
  matched: boolean // true neu parse duoc so lan/ngay tu frequency
}

export function previewReminderSlots(frequency: string | null | undefined): ReminderPreview {
  if (frequency) {
    const m = DOSES_PER_DAY_REGEX.exec(frequency)
    if (m) {
      const doses = Number(m[1])
      if (Number.isFinite(doses) && doses >= 1) {
        const clamped = Math.min(doses, DEFAULT_DAILY_SLOTS.length - 1)
        return { slots: DEFAULT_DAILY_SLOTS[clamped], matched: true }
      }
    }
  }
  return { slots: DEFAULT_DAILY_SLOTS[1], matched: false }
}

// Mirror logic ParseDurationDays cua backend trong PharmacistPrescriptionVerificationService.
// Parse "N ngay" -> so ngay. Khong khop -> null (EndDate mo).
const DURATION_DAYS_REGEX = /(\d+)\s*ng(?:à|a)y/i

export function parseDurationDays(duration: string | null | undefined): number | null {
  if (!duration) return null
  const m = DURATION_DAYS_REGEX.exec(duration)
  if (!m) return null
  const n = Number(m[1])
  return Number.isFinite(n) && n >= 1 ? n : null
}

