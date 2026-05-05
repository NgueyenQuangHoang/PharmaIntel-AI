// =============================================================================
// Medications slice - 2 list cache:
//   - 'catalog': danh sach hien tai cua MedicineCabinetPage (filter theo category)
//   - 'featured': featured + bestseller cho home FeaturedMedications
// =============================================================================
import { createAsyncThunk, createSlice } from '@reduxjs/toolkit'
import axios from 'axios'
import { medicationsApi } from './medications-api'
import type { MedicationListItem, MedicationListQuery } from './types'

type ListBucket = {
  items: MedicationListItem[]
  status: 'idle' | 'loading' | 'success' | 'error'
  error: string | null
  totalCount: number
}

export type MedicationsState = {
  catalog: ListBucket
  featured: ListBucket
}

const emptyBucket: ListBucket = { items: [], status: 'idle', error: null, totalCount: 0 }

const initialState: MedicationsState = {
  catalog: { ...emptyBucket },
  featured: { ...emptyBucket },
}

function extractError(err: unknown, fallback: string): string {
  if (axios.isAxiosError(err)) {
    const data = err.response?.data as { title?: string; detail?: string; message?: string } | undefined
    return data?.detail ?? data?.title ?? data?.message ?? err.message ?? fallback
  }
  return fallback
}

export const fetchCatalogThunk = createAsyncThunk<
  { items: MedicationListItem[]; total: number },
  MedicationListQuery,
  { rejectValue: string }
>('medications/fetchCatalog', async (query, { rejectWithValue }) => {
  try {
    const paged = await medicationsApi.list({
      page: 1,
      pageSize: 24,
      isActive: true,
      sortBy: 'name',
      ...query,
    })
    return { items: paged.items, total: paged.totalCount }
  } catch (err) {
    return rejectWithValue(extractError(err, 'Khong tai duoc danh sach thuoc'))
  }
})

export const fetchFeaturedThunk = createAsyncThunk<
  MedicationListItem[],
  void,
  { rejectValue: string }
>('medications/fetchFeatured', async (_, { rejectWithValue }) => {
  try {
    const paged = await medicationsApi.list({
      page: 1,
      pageSize: 8,
      isActive: true,
      isFeatured: true,
    })
    return paged.items
  } catch (err) {
    return rejectWithValue(extractError(err, 'Khong tai duoc thuoc noi bat'))
  }
})

const medicationsSlice = createSlice({
  name: 'medications',
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchCatalogThunk.pending, (state) => {
        state.catalog.status = 'loading'
        state.catalog.error = null
      })
      .addCase(fetchCatalogThunk.fulfilled, (state, action) => {
        state.catalog.items = action.payload.items
        state.catalog.totalCount = action.payload.total
        state.catalog.status = 'success'
      })
      .addCase(fetchCatalogThunk.rejected, (state, action) => {
        state.catalog.status = 'error'
        state.catalog.error = action.payload ?? 'Khong tai duoc danh sach thuoc'
      })

      .addCase(fetchFeaturedThunk.pending, (state) => {
        state.featured.status = 'loading'
        state.featured.error = null
      })
      .addCase(fetchFeaturedThunk.fulfilled, (state, action) => {
        state.featured.items = action.payload
        state.featured.totalCount = action.payload.length
        state.featured.status = 'success'
      })
      .addCase(fetchFeaturedThunk.rejected, (state, action) => {
        state.featured.status = 'error'
        state.featured.error = action.payload ?? 'Khong tai duoc thuoc noi bat'
      })
  },
})

export default medicationsSlice.reducer
