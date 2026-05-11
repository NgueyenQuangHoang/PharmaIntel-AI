import type { Paged } from '@/features/categories/types'

export type MedicationReminderStatus =
  | 'active'
  | 'paused'
  | 'completed'
  | 'cancelled'

export type MedicationReminderLogStatus =
  | 'taken'
  | 'missed'
  | 'skipped'

export type MedicationReminderListItem = {
  id: number
  prescriptionItemId: number | null
  medicationName: string
  frequencyType: string
  reminderTime: string  // "HH:mm:ss"
  startDate: string     // "YYYY-MM-DD"
  endDate: string | null
  status: MedicationReminderStatus | string
  logCount: number
  createdAt: string
  updatedAt: string
}

export type MedicationReminderUpdateRequest = {
  prescriptionItemId?: number | null
  medicationName?: string | null
  frequencyType: string
  reminderTime: string
  startDate?: string | null
  endDate?: string | null
  status: MedicationReminderStatus | string
}

export type MedicationReminderLogCreateRequest = {
  scheduledAt: string
  status: MedicationReminderLogStatus
}

export type MedicationReminderListQuery = {
  page?: number
  pageSize?: number
  status?: MedicationReminderStatus | ''
  q?: string
}

export type MedicationRemindersPaged = Paged<MedicationReminderListItem>
