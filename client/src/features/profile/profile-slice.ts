import { createAsyncThunk, createSlice } from '@reduxjs/toolkit'
import axios from 'axios'
import { diagnosticApi } from '@/features/diagnostic/diagnostic-api'
import { profileApi } from './profile-api'
import type { ProfileState, PrescriptionListItem, MedicationReminderListItem, HealthMetric } from './types'
import type { DiagnosticSessionListItem } from '@/features/diagnostic/types'
import type { Paged } from '@/features/categories/types'

const initialState: ProfileState = {
  diagnostics: { data: null, status: 'idle', error: null },
  prescriptions: { data: null, status: 'idle', error: null },
  reminders: { data: null, status: 'idle', error: null },
  healthMetrics: { data: null, status: 'idle', error: null },
}

function extractError(err: unknown, fallback: string): string {
  if (axios.isAxiosError(err)) {
    const data = err.response?.data as { title?: string; detail?: string; message?: string } | undefined
    return data?.detail ?? data?.title ?? data?.message ?? err.message ?? fallback
  }
  return fallback
}

export const fetchDiagnosisHistoryThunk = createAsyncThunk<
  Paged<DiagnosticSessionListItem>,
  { page?: number; pageSize?: number; status?: string } | undefined,
  { rejectValue: string }
>('profile/fetchDiagnosisHistory', async (params, { rejectWithValue }) => {
  try {
    return await diagnosticApi.listMySessions(params)
  } catch (err) {
    return rejectWithValue(extractError(err, 'Không tải được lịch sử chẩn đoán'))
  }
})

export const fetchPrescriptionsThunk = createAsyncThunk<
  Paged<PrescriptionListItem>,
  { page?: number; pageSize?: number; status?: string } | undefined,
  { rejectValue: string }
>('profile/fetchPrescriptions', async (params, { rejectWithValue }) => {
  try {
    return await profileApi.listMyPrescriptions(params)
  } catch (err) {
    return rejectWithValue(extractError(err, 'Không tải được đơn thuốc'))
  }
})

export const fetchMedicationRemindersThunk = createAsyncThunk<
  Paged<MedicationReminderListItem>,
  { page?: number; pageSize?: number; status?: string; q?: string } | undefined,
  { rejectValue: string }
>('profile/fetchMedicationReminders', async (params, { rejectWithValue }) => {
  try {
    return await profileApi.listMyMedicationReminders(params)
  } catch (err) {
    return rejectWithValue(extractError(err, 'Không tải được lịch nhắc thuốc'))
  }
})

export const fetchHealthMetricsThunk = createAsyncThunk<
  Paged<HealthMetric>,
  { page?: number; pageSize?: number; metricType?: string; fromDate?: string; toDate?: string } | undefined,
  { rejectValue: string }
>('profile/fetchHealthMetrics', async (params, { rejectWithValue }) => {
  try {
    return await profileApi.listMyHealthMetrics(params)
  } catch (err) {
    return rejectWithValue(extractError(err, 'Không tải được chỉ số sức khỏe'))
  }
})

const profileSlice = createSlice({
  name: 'profile',
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchDiagnosisHistoryThunk.pending, (state) => {
        state.diagnostics.status = 'loading'
        state.diagnostics.error = null
      })
      .addCase(fetchDiagnosisHistoryThunk.fulfilled, (state, action) => {
        state.diagnostics.status = 'success'
        state.diagnostics.data = action.payload
      })
      .addCase(fetchDiagnosisHistoryThunk.rejected, (state, action) => {
        state.diagnostics.status = 'error'
        state.diagnostics.error = action.payload ?? 'Lỗi tải dữ liệu'
      })
      .addCase(fetchPrescriptionsThunk.pending, (state) => {
        state.prescriptions.status = 'loading'
        state.prescriptions.error = null
      })
      .addCase(fetchPrescriptionsThunk.fulfilled, (state, action) => {
        state.prescriptions.status = 'success'
        state.prescriptions.data = action.payload
      })
      .addCase(fetchPrescriptionsThunk.rejected, (state, action) => {
        state.prescriptions.status = 'error'
        state.prescriptions.error = action.payload ?? 'Lỗi tải dữ liệu'
      })
      .addCase(fetchMedicationRemindersThunk.pending, (state) => {
        state.reminders.status = 'loading'
        state.reminders.error = null
      })
      .addCase(fetchMedicationRemindersThunk.fulfilled, (state, action) => {
        state.reminders.status = 'success'
        state.reminders.data = action.payload
      })
      .addCase(fetchMedicationRemindersThunk.rejected, (state, action) => {
        state.reminders.status = 'error'
        state.reminders.error = action.payload ?? 'Lỗi tải dữ liệu'
      })
      .addCase(fetchHealthMetricsThunk.pending, (state) => {
        state.healthMetrics.status = 'loading'
        state.healthMetrics.error = null
      })
      .addCase(fetchHealthMetricsThunk.fulfilled, (state, action) => {
        state.healthMetrics.status = 'success'
        state.healthMetrics.data = action.payload
      })
      .addCase(fetchHealthMetricsThunk.rejected, (state, action) => {
        state.healthMetrics.status = 'error'
        state.healthMetrics.error = action.payload ?? 'Lỗi tải dữ liệu'
      })
  },
})

export default profileSlice.reducer
