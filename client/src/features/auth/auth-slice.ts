// =============================================================================
// Auth slice - quan ly token + user trong Redux, hydrate tu localStorage
// =============================================================================
import { createAsyncThunk, createSlice, type PayloadAction } from '@reduxjs/toolkit'
import axios from 'axios'
import { authApi } from './auth-api'
import { clearToken, getToken, setToken } from './token-storage'
import type { AuthResponse, LoginRequest, RegisterRequest, UserInfo } from './types'

export type AuthStatus = 'idle' | 'loading' | 'authenticated' | 'error'

export type AuthState = {
  token: string | null
  user: UserInfo | null
  status: AuthStatus
  error: string | null
}

const initialState: AuthState = {
  token: getToken(),
  user: null,
  status: getToken() ? 'loading' : 'idle',
  error: null,
}

function extractError(err: unknown, fallback: string): string {
  if (axios.isAxiosError(err)) {
    const data = err.response?.data as { title?: string; detail?: string; message?: string } | undefined
    return data?.detail ?? data?.title ?? data?.message ?? err.message ?? fallback
  }
  return fallback
}

export const loginThunk = createAsyncThunk<AuthResponse, LoginRequest, { rejectValue: string }>(
  'auth/login',
  async (body, { rejectWithValue }) => {
    try {
      return await authApi.login(body)
    } catch (err) {
      return rejectWithValue(extractError(err, 'Dang nhap that bai'))
    }
  },
)

export const registerThunk = createAsyncThunk<AuthResponse, RegisterRequest, { rejectValue: string }>(
  'auth/register',
  async (body, { rejectWithValue }) => {
    try {
      return await authApi.register(body)
    } catch (err) {
      return rejectWithValue(extractError(err, 'Dang ky that bai'))
    }
  },
)

export const fetchMeThunk = createAsyncThunk<UserInfo, void, { rejectValue: string }>(
  'auth/fetchMe',
  async (_, { rejectWithValue }) => {
    try {
      return await authApi.me()
    } catch (err) {
      return rejectWithValue(extractError(err, 'Khong lay duoc thong tin user'))
    }
  },
)

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    logout(state) {
      clearToken()
      state.token = null
      state.user = null
      state.status = 'idle'
      state.error = null
    },
    clearError(state) {
      state.error = null
    },
  },
  extraReducers: (builder) => {
    const handleAuthSuccess = (state: AuthState, action: PayloadAction<AuthResponse>) => {
      setToken(action.payload.accessToken)
      state.token = action.payload.accessToken
      state.user = action.payload.user
      state.status = 'authenticated'
      state.error = null
    }

    builder
      .addCase(loginThunk.pending, (state) => {
        state.status = 'loading'
        state.error = null
      })
      .addCase(loginThunk.fulfilled, handleAuthSuccess)
      .addCase(loginThunk.rejected, (state, action) => {
        state.status = 'error'
        state.error = action.payload ?? 'Dang nhap that bai'
      })

      .addCase(registerThunk.pending, (state) => {
        state.status = 'loading'
        state.error = null
      })
      .addCase(registerThunk.fulfilled, handleAuthSuccess)
      .addCase(registerThunk.rejected, (state, action) => {
        state.status = 'error'
        state.error = action.payload ?? 'Dang ky that bai'
      })

      .addCase(fetchMeThunk.pending, (state) => {
        state.status = 'loading'
        state.error = null
      })
      .addCase(fetchMeThunk.fulfilled, (state, action) => {
        state.user = action.payload
        state.status = 'authenticated'
        state.error = null
      })
      .addCase(fetchMeThunk.rejected, (state) => {
        clearToken()
        state.token = null
        state.user = null
        state.status = 'idle'
      })
  },
})

export const { logout, clearError } = authSlice.actions
export default authSlice.reducer
