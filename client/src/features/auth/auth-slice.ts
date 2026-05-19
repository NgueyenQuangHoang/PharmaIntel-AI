// =============================================================================
// Auth slice - quan ly token + user trong Redux, hydrate tu localStorage
// =============================================================================
import { createAsyncThunk, createSlice, type PayloadAction } from '@reduxjs/toolkit'
import axios from 'axios'
import { authApi } from './auth-api'
import {
  clearRefreshToken,
  clearToken,
  getRefreshToken,
  getToken,
  setRefreshToken,
  setToken,
} from './token-storage'
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

export const loginWithGoogleThunk = createAsyncThunk<AuthResponse, string, { rejectValue: string }>(
  'auth/loginWithGoogle',
  async (idToken, { rejectWithValue }) => {
    try {
      return await authApi.loginWithGoogle(idToken)
    } catch (err) {
      return rejectWithValue(extractError(err, 'Dang nhap Google that bai'))
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

// Logout: revoke refresh token o BE (best-effort) -> clear local state.
export const logoutThunk = createAsyncThunk<void, void>(
  'auth/logoutThunk',
  async (_, { dispatch }) => {
    const refreshToken = getRefreshToken()
    if (refreshToken) {
      try {
        await authApi.logout(refreshToken)
      } catch {
        /* ignore - BE co the da revoke hoac network fail; van clear local */
      }
    }
    dispatch(logout())
  },
)

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    logout(state) {
      clearToken()
      clearRefreshToken()
      state.token = null
      state.user = null
      state.status = 'idle'
      state.error = null
    },
    clearError(state) {
      state.error = null
    },
    // Patch user state sau khi update profile thanh cong, khong can refetch /auth/me.
    userUpdated(state, action: PayloadAction<Partial<UserInfo>>) {
      if (state.user) {
        state.user = { ...state.user, ...action.payload }
      }
    },
  },
  extraReducers: (builder) => {
    const handleAuthSuccess = (state: AuthState, action: PayloadAction<AuthResponse>) => {
      setToken(action.payload.accessToken)
      setRefreshToken(action.payload.refreshToken)
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

      .addCase(loginWithGoogleThunk.pending, (state) => {
        state.status = 'loading'
        state.error = null
      })
      .addCase(loginWithGoogleThunk.fulfilled, handleAuthSuccess)
      .addCase(loginWithGoogleThunk.rejected, (state, action) => {
        state.status = 'error'
        state.error = action.payload ?? 'Dang nhap Google that bai'
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

export const { logout, clearError, userUpdated } = authSlice.actions
export default authSlice.reducer
