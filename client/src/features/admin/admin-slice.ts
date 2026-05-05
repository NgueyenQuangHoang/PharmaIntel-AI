// =============================================================================
// Admin slice - thunks cho stats overview va danh sach user.
// Cac thao tac mutating (updateRole/Status/delete) khong qua slice ma goi
// truc tiep adminApi tu component, sau do refetch list.
// =============================================================================
import { createAsyncThunk, createSlice } from '@reduxjs/toolkit'
import axios from 'axios'
import { adminApi } from './admin-api'
import type {
  AdminStatsOverview,
  AdminUser,
  AdminUserListQuery,
  OrdersByStatus,
  RevenuePoint,
  RevenueRange,
  TopMedication,
} from './types'

type Status = 'idle' | 'loading' | 'success' | 'error'

export type AdminState = {
  overview: { data: AdminStatsOverview | null; status: Status; error: string | null }
  revenue: { data: RevenuePoint[]; range: RevenueRange; status: Status; error: string | null }
  topMedications: { data: TopMedication[]; status: Status; error: string | null }
  ordersByStatus: { data: OrdersByStatus[]; status: Status; error: string | null }
  users: {
    items: AdminUser[]
    totalCount: number
    page: number
    pageSize: number
    status: Status
    error: string | null
  }
}

const initialState: AdminState = {
  overview: { data: null, status: 'idle', error: null },
  revenue: { data: [], range: '7d', status: 'idle', error: null },
  topMedications: { data: [], status: 'idle', error: null },
  ordersByStatus: { data: [], status: 'idle', error: null },
  users: { items: [], totalCount: 0, page: 1, pageSize: 20, status: 'idle', error: null },
}

function extractError(err: unknown, fallback: string): string {
  if (axios.isAxiosError(err)) {
    const data = err.response?.data as { title?: string; detail?: string; message?: string } | undefined
    return data?.detail ?? data?.title ?? data?.message ?? err.message ?? fallback
  }
  return fallback
}

export const fetchOverviewThunk = createAsyncThunk<AdminStatsOverview, void, { rejectValue: string }>(
  'admin/fetchOverview',
  async (_, { rejectWithValue }) => {
    try {
      return await adminApi.stats.overview()
    } catch (err) {
      return rejectWithValue(extractError(err, 'Khong tai duoc thong ke tong quan'))
    }
  },
)

export const fetchRevenueThunk = createAsyncThunk<
  { range: RevenueRange; data: RevenuePoint[] },
  RevenueRange,
  { rejectValue: string }
>('admin/fetchRevenue', async (range, { rejectWithValue }) => {
  try {
    const data = await adminApi.stats.revenue(range)
    return { range, data }
  } catch (err) {
    return rejectWithValue(extractError(err, 'Khong tai duoc doanh thu'))
  }
})

export const fetchTopMedicationsThunk = createAsyncThunk<TopMedication[], number | undefined, { rejectValue: string }>(
  'admin/fetchTopMedications',
  async (limit, { rejectWithValue }) => {
    try {
      return await adminApi.stats.topMedications(limit ?? 10)
    } catch (err) {
      return rejectWithValue(extractError(err, 'Khong tai duoc top san pham'))
    }
  },
)

export const fetchOrdersByStatusThunk = createAsyncThunk<OrdersByStatus[], void, { rejectValue: string }>(
  'admin/fetchOrdersByStatus',
  async (_, { rejectWithValue }) => {
    try {
      return await adminApi.stats.ordersByStatus()
    } catch (err) {
      return rejectWithValue(extractError(err, 'Khong tai duoc trang thai don hang'))
    }
  },
)

export const fetchAdminUsersThunk = createAsyncThunk<
  { items: AdminUser[]; totalCount: number; page: number; pageSize: number },
  AdminUserListQuery,
  { rejectValue: string }
>('admin/fetchUsers', async (query, { rejectWithValue }) => {
  try {
    const paged = await adminApi.users.list({ page: 1, pageSize: 20, ...query })
    return { items: paged.items, totalCount: paged.totalCount, page: paged.page, pageSize: paged.pageSize }
  } catch (err) {
    return rejectWithValue(extractError(err, 'Khong tai duoc danh sach nguoi dung'))
  }
})

const adminSlice = createSlice({
  name: 'admin',
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchOverviewThunk.pending, (s) => {
        s.overview.status = 'loading'
        s.overview.error = null
      })
      .addCase(fetchOverviewThunk.fulfilled, (s, a) => {
        s.overview.status = 'success'
        s.overview.data = a.payload
      })
      .addCase(fetchOverviewThunk.rejected, (s, a) => {
        s.overview.status = 'error'
        s.overview.error = a.payload ?? 'Loi'
      })

      .addCase(fetchRevenueThunk.pending, (s) => {
        s.revenue.status = 'loading'
        s.revenue.error = null
      })
      .addCase(fetchRevenueThunk.fulfilled, (s, a) => {
        s.revenue.status = 'success'
        s.revenue.data = a.payload.data
        s.revenue.range = a.payload.range
      })
      .addCase(fetchRevenueThunk.rejected, (s, a) => {
        s.revenue.status = 'error'
        s.revenue.error = a.payload ?? 'Loi'
      })

      .addCase(fetchTopMedicationsThunk.pending, (s) => {
        s.topMedications.status = 'loading'
        s.topMedications.error = null
      })
      .addCase(fetchTopMedicationsThunk.fulfilled, (s, a) => {
        s.topMedications.status = 'success'
        s.topMedications.data = a.payload
      })
      .addCase(fetchTopMedicationsThunk.rejected, (s, a) => {
        s.topMedications.status = 'error'
        s.topMedications.error = a.payload ?? 'Loi'
      })

      .addCase(fetchOrdersByStatusThunk.pending, (s) => {
        s.ordersByStatus.status = 'loading'
        s.ordersByStatus.error = null
      })
      .addCase(fetchOrdersByStatusThunk.fulfilled, (s, a) => {
        s.ordersByStatus.status = 'success'
        s.ordersByStatus.data = a.payload
      })
      .addCase(fetchOrdersByStatusThunk.rejected, (s, a) => {
        s.ordersByStatus.status = 'error'
        s.ordersByStatus.error = a.payload ?? 'Loi'
      })

      .addCase(fetchAdminUsersThunk.pending, (s) => {
        s.users.status = 'loading'
        s.users.error = null
      })
      .addCase(fetchAdminUsersThunk.fulfilled, (s, a) => {
        s.users.status = 'success'
        s.users.items = a.payload.items
        s.users.totalCount = a.payload.totalCount
        s.users.page = a.payload.page
        s.users.pageSize = a.payload.pageSize
      })
      .addCase(fetchAdminUsersThunk.rejected, (s, a) => {
        s.users.status = 'error'
        s.users.error = a.payload ?? 'Loi'
      })
  },
})

export default adminSlice.reducer
