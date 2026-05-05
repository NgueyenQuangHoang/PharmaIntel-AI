// =============================================================================
// Categories slice - cache flat list categories (root only, sap xep theo display order)
// =============================================================================
import { createAsyncThunk, createSlice } from '@reduxjs/toolkit'
import axios from 'axios'
import { categoriesApi } from './categories-api'
import type { Category } from './types'

export type CategoriesState = {
  items: Category[]
  status: 'idle' | 'loading' | 'success' | 'error'
  error: string | null
}

const initialState: CategoriesState = {
  items: [],
  status: 'idle',
  error: null,
}

function extractError(err: unknown, fallback: string): string {
  if (axios.isAxiosError(err)) {
    const data = err.response?.data as { title?: string; detail?: string; message?: string } | undefined
    return data?.detail ?? data?.title ?? data?.message ?? err.message ?? fallback
  }
  return fallback
}

export const fetchCategoriesThunk = createAsyncThunk<Category[], void, { rejectValue: string }>(
  'categories/fetch',
  async (_, { rejectWithValue }) => {
    try {
      const paged = await categoriesApi.list({ pageSize: 100, isActive: true, rootOnly: true })
      return paged.items.slice().sort((a, b) => a.displayOrder - b.displayOrder)
    } catch (err) {
      return rejectWithValue(extractError(err, 'Khong tai duoc danh muc'))
    }
  },
)

const categoriesSlice = createSlice({
  name: 'categories',
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchCategoriesThunk.pending, (state) => {
        state.status = 'loading'
        state.error = null
      })
      .addCase(fetchCategoriesThunk.fulfilled, (state, action) => {
        state.items = action.payload
        state.status = 'success'
      })
      .addCase(fetchCategoriesThunk.rejected, (state, action) => {
        state.status = 'error'
        state.error = action.payload ?? 'Khong tai duoc danh muc'
      })
  },
})

export default categoriesSlice.reducer
