import { httpClient } from '@/services/http-client'
import type {
  MedicationReminderListItem,
  MedicationReminderListQuery,
  MedicationReminderLogCreateRequest,
  MedicationReminderUpdateRequest,
  MedicationRemindersPaged,
} from './types'

export const medicationRemindersApi = {
  list: async (query: MedicationReminderListQuery = {}) => {
    const res = await httpClient.get<MedicationRemindersPaged>('/medication-reminders', {
      params: { page: 1, pageSize: 50, ...query },
    })
    return res.data
  },

  update: async (id: number, body: MedicationReminderUpdateRequest) => {
    const res = await httpClient.put<MedicationReminderListItem>(
      `/medication-reminders/${id}`,
      body,
    )
    return res.data
  },

  delete: async (id: number) => {
    await httpClient.delete(`/medication-reminders/${id}`)
  },

  addLog: async (id: number, body: MedicationReminderLogCreateRequest) => {
    const res = await httpClient.post(`/medication-reminders/${id}/logs`, body)
    return res.data
  },

  listLogs: async (
    id: number,
    query: { page?: number; pageSize?: number; status?: string; fromDate?: string; toDate?: string } = {},
  ) => {
    const res = await httpClient.get(`/medication-reminders/${id}/logs`, {
      params: { page: 1, pageSize: 20, ...query },
    })
    return res.data
  },
}
