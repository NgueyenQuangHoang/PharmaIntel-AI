import type { DiagnosticSessionListItem } from '@/features/diagnostic/types'
import type { Paged } from '@/features/categories/types'

export type PrescriptionItem = {
  id: number
  prescriptionId: number
  medicationId: number | null
  medicationName: string
  dosage: string | null
  frequency: string | null
  duration: string | null
}

export type PrescriptionListItem = {
  id: number
  doctorId: number | null
  doctorNameSnapshot: string | null
  title: string | null
  prescribedDate: string | null
  status: string
  verificationStatus: string
  itemCount: number
  createdAt: string
  updatedAt: string
}

export type Prescription = PrescriptionListItem & {
  items: PrescriptionItem[]
}

export type MedicationReminderListItem = {
  id: number
  prescriptionItemId: number | null
  medicationName: string
  frequencyType: string
  reminderTime: string // TimeOnly translates to string (e.g. "08:00:00")
  status: string
  logCount: number
  createdAt: string
  updatedAt: string
}

export type HealthMetric = {
  id: number
  metricType: string
  valueNumber: number
  valueNumber2: number | null
  unit: string | null
  notes: string | null
  recordedAt: string
}

export type ProfileState = {
  diagnostics: {
    data: Paged<DiagnosticSessionListItem> | null
    status: 'idle' | 'loading' | 'success' | 'error'
    error: string | null
  }
  prescriptions: {
    data: Paged<PrescriptionListItem> | null
    status: 'idle' | 'loading' | 'success' | 'error'
    error: string | null
  }
  reminders: {
    data: Paged<MedicationReminderListItem> | null
    status: 'idle' | 'loading' | 'success' | 'error'
    error: string | null
  }
  healthMetrics: {
    data: Paged<HealthMetric> | null
    status: 'idle' | 'loading' | 'success' | 'error'
    error: string | null
  }
}
