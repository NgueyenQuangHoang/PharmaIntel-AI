// =============================================================================
// Cart slice - giu cart hien tai. Moi mutation goi BE va replace state bang
// CartDto moi (BE tinh san subtotal/total/lineTotal).
// =============================================================================
import { createAsyncThunk, createSlice } from '@reduxjs/toolkit'
import axios from 'axios'
import { cartApi } from './cart-api'
import type { Cart } from './types'

export type CartState = {
  cart: Cart | null
  status: 'idle' | 'loading' | 'success' | 'error'
  mutating: boolean
  pendingMedicationIds: number[]    // dung de disable nut khi dang submit per-item
  error: string | null
}

const initialState: CartState = {
  cart: null,
  status: 'idle',
  mutating: false,
  pendingMedicationIds: [],
  error: null,
}

function extractError(err: unknown, fallback: string): string {
  if (axios.isAxiosError(err)) {
    const data = err.response?.data as { title?: string; detail?: string; message?: string } | undefined
    return data?.detail ?? data?.title ?? data?.message ?? err.message ?? fallback
  }
  return fallback
}

export const fetchCartThunk = createAsyncThunk<Cart, void, { rejectValue: string }>(
  'cart/fetch',
  async (_, { rejectWithValue }) => {
    try {
      return await cartApi.get()
    } catch (err) {
      return rejectWithValue(extractError(err, 'Khong tai duoc gio hang'))
    }
  },
)

export const addToCartThunk = createAsyncThunk<
  Cart,
  { medicationId: number; quantity?: number },
  { rejectValue: string }
>('cart/add', async ({ medicationId, quantity = 1 }, { rejectWithValue }) => {
  try {
    return await cartApi.addItem(medicationId, quantity)
  } catch (err) {
    return rejectWithValue(extractError(err, 'Khong them duoc vao gio'))
  }
})

export const updateCartItemThunk = createAsyncThunk<
  Cart,
  { medicationId: number; quantity: number },
  { rejectValue: string }
>('cart/update', async ({ medicationId, quantity }, { rejectWithValue }) => {
  try {
    return await cartApi.updateItem(medicationId, quantity)
  } catch (err) {
    return rejectWithValue(extractError(err, 'Khong cap nhat duoc gio'))
  }
})

export const removeCartItemThunk = createAsyncThunk<Cart, number, { rejectValue: string }>(
  'cart/remove',
  async (medicationId, { rejectWithValue }) => {
    try {
      return await cartApi.removeItem(medicationId)
    } catch (err) {
      return rejectWithValue(extractError(err, 'Khong xoa duoc item'))
    }
  },
)

export const clearCartThunk = createAsyncThunk<void, void, { rejectValue: string }>(
  'cart/clear',
  async (_, { rejectWithValue }) => {
    try {
      await cartApi.clear()
    } catch (err) {
      return rejectWithValue(extractError(err, 'Khong xoa duoc gio'))
    }
  },
)

const cartSlice = createSlice({
  name: 'cart',
  initialState,
  reducers: {
    cartReset(state) {
      state.cart = null
      state.status = 'idle'
      state.mutating = false
      state.pendingMedicationIds = []
      state.error = null
    },
  },
  extraReducers: (builder) => {
    const setCart = (state: CartState, cart: Cart) => {
      state.cart = cart
      state.status = 'success'
      state.error = null
    }

    builder
      .addCase(fetchCartThunk.pending, (state) => {
        state.status = 'loading'
        state.error = null
      })
      .addCase(fetchCartThunk.fulfilled, (state, action) => setCart(state, action.payload))
      .addCase(fetchCartThunk.rejected, (state, action) => {
        state.status = 'error'
        state.error = action.payload ?? 'Khong tai duoc gio hang'
      })

      .addCase(addToCartThunk.pending, (state, action) => {
        state.mutating = true
        if (!state.pendingMedicationIds.includes(action.meta.arg.medicationId)) {
          state.pendingMedicationIds.push(action.meta.arg.medicationId)
        }
      })
      .addCase(addToCartThunk.fulfilled, (state, action) => {
        state.mutating = false
        state.pendingMedicationIds = state.pendingMedicationIds.filter(
          (id) => id !== action.meta.arg.medicationId,
        )
        setCart(state, action.payload)
      })
      .addCase(addToCartThunk.rejected, (state, action) => {
        state.mutating = false
        state.pendingMedicationIds = state.pendingMedicationIds.filter(
          (id) => id !== action.meta.arg.medicationId,
        )
        state.error = action.payload ?? 'Khong them duoc vao gio'
      })

      .addCase(updateCartItemThunk.pending, (state, action) => {
        state.mutating = true
        if (!state.pendingMedicationIds.includes(action.meta.arg.medicationId)) {
          state.pendingMedicationIds.push(action.meta.arg.medicationId)
        }
      })
      .addCase(updateCartItemThunk.fulfilled, (state, action) => {
        state.mutating = false
        state.pendingMedicationIds = state.pendingMedicationIds.filter(
          (id) => id !== action.meta.arg.medicationId,
        )
        setCart(state, action.payload)
      })
      .addCase(updateCartItemThunk.rejected, (state, action) => {
        state.mutating = false
        state.pendingMedicationIds = state.pendingMedicationIds.filter(
          (id) => id !== action.meta.arg.medicationId,
        )
        state.error = action.payload ?? 'Khong cap nhat duoc'
      })

      .addCase(removeCartItemThunk.pending, (state, action) => {
        state.mutating = true
        if (!state.pendingMedicationIds.includes(action.meta.arg)) {
          state.pendingMedicationIds.push(action.meta.arg)
        }
      })
      .addCase(removeCartItemThunk.fulfilled, (state, action) => {
        state.mutating = false
        state.pendingMedicationIds = state.pendingMedicationIds.filter(
          (id) => id !== action.meta.arg,
        )
        setCart(state, action.payload)
      })
      .addCase(removeCartItemThunk.rejected, (state, action) => {
        state.mutating = false
        state.pendingMedicationIds = state.pendingMedicationIds.filter(
          (id) => id !== action.meta.arg,
        )
        state.error = action.payload ?? 'Khong xoa duoc'
      })

      .addCase(clearCartThunk.pending, (state) => {
        state.mutating = true
      })
      .addCase(clearCartThunk.fulfilled, (state) => {
        state.mutating = false
        if (state.cart) {
          state.cart = {
            ...state.cart,
            items: [],
            totalItems: 0,
            distinctItems: 0,
            subtotal: 0,
            totalDiscount: 0,
            total: 0,
            hasUnavailableItems: false,
            hasPrescriptionRequired: false,
          }
        }
      })
      .addCase(clearCartThunk.rejected, (state, action) => {
        state.mutating = false
        state.error = action.payload ?? 'Khong xoa duoc'
      })

      // Cross-slice: logout xoa gio hang local. Khong import auth-slice de tranh circular.
      .addMatcher(
        (action): action is { type: string } => action.type === 'auth/logout',
        (state) => {
          state.cart = null
          state.status = 'idle'
          state.mutating = false
          state.pendingMedicationIds = []
          state.error = null
        },
      )
  },
})

export const { cartReset } = cartSlice.actions
export default cartSlice.reducer
