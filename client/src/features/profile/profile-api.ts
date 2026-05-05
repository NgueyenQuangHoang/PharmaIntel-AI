import { httpClient } from '@/services/http-client'
import type { PrescriptionListItem, MedicationReminderListItem, HealthMetric } from './types'
import type { Paged } from '@/features/categories/types'

export const profileApi = {
  listMyPrescriptions: async (params?: { page?: number; pageSize?: number; status?: string }) => {
    const res = await httpClient.get<Paged<PrescriptionListItem>>('/prescriptions/my', { params })
    return res.data
  },
  listMyMedicationReminders: async (params?: { page?: number; pageSize?: number; status?: string; q?: string }) => {
    const res = await httpClient.get<Paged<MedicationReminderListItem>>('/medication-reminders', { params })
    return res.data
  },
  listMyHealthMetrics: async (params?: { page?: number; pageSize?: number; metricType?: string; fromDate?: string; toDate?: string }) => {
    const res = await httpClient.get<Paged<HealthMetric>>('/health-metrics', { params })
    return res.data
  },
}
