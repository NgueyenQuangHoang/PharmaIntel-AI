// =============================================================================
// Diagnostic slice - giu symptoms catalog + current session.
// Flow:
//   1. fetchSymptomsThunk -> hien chip o sidebar
//   2. createSessionThunk -> session moi (BE tu them welcome message tu AI)
//   3. addMessageThunk -> goi them tin (AI tu reply trong cung response cua server
//      qua refresh getSession)
//   4. completeSessionThunk -> AI engine sinh ket qua, tra session co .result
// =============================================================================
import { createAsyncThunk, createSlice } from '@reduxjs/toolkit'
import axios from 'axios'
import { diagnosticApi } from './diagnostic-api'
import type { DiagnosticSession, Symptom } from './types'

type Bucket = 'idle' | 'loading' | 'success' | 'error'

export type DiagnosticState = {
  symptoms: Symptom[]
  symptomsStatus: Bucket
  selectedSymptomIds: number[]
  currentSession: DiagnosticSession | null
  sessionStatus: Bucket          // tao session / fetch session
  completing: boolean            // dang phan tich (complete API)
  sendingMessage: boolean
  error: string | null
}

const initialState: DiagnosticState = {
  symptoms: [],
  symptomsStatus: 'idle',
  selectedSymptomIds: [],
  currentSession: null,
  sessionStatus: 'idle',
  completing: false,
  sendingMessage: false,
  error: null,
}

function extractError(err: unknown, fallback: string): string {
  if (axios.isAxiosError(err)) {
    const data = err.response?.data as { title?: string; detail?: string; message?: string } | undefined
    return data?.detail ?? data?.title ?? data?.message ?? err.message ?? fallback
  }
  return fallback
}

export const fetchSymptomsThunk = createAsyncThunk<Symptom[], void, { rejectValue: string }>(
  'diagnostic/fetchSymptoms',
  async (_, { rejectWithValue }) => {
    try {
      return await diagnosticApi.listSymptoms()
    } catch (err) {
      return rejectWithValue(extractError(err, 'Khong tai duoc trieu chung'))
    }
  },
)

export const createSessionThunk = createAsyncThunk<
  DiagnosticSession,
  { symptomIds: number[]; initialMessage?: string },
  { rejectValue: string }
>('diagnostic/createSession', async (body, { rejectWithValue }) => {
  try {
    return await diagnosticApi.createSession(body)
  } catch (err) {
    return rejectWithValue(extractError(err, 'Khong tao duoc phien chan doan'))
  }
})

export const fetchSessionThunk = createAsyncThunk<DiagnosticSession, number, { rejectValue: string }>(
  'diagnostic/fetchSession',
  async (id, { rejectWithValue }) => {
    try {
      return await diagnosticApi.getSession(id)
    } catch (err) {
      return rejectWithValue(extractError(err, 'Khong tai duoc phien'))
    }
  },
)

export const sendMessageThunk = createAsyncThunk<
  DiagnosticSession,
  { sessionId: number; content: string },
  { rejectValue: string }
>('diagnostic/sendMessage', async ({ sessionId, content }, { rejectWithValue }) => {
  try {
    await diagnosticApi.addMessage(sessionId, content)
    // Refresh full session de lay luon AI reply (BE auto-add)
    return await diagnosticApi.getSession(sessionId)
  } catch (err) {
    return rejectWithValue(extractError(err, 'Khong gui duoc tin nhan'))
  }
})

// sendChatMessageThunk: dung cho UI chat. Neu chua co session -> tao session
// trong (khong InitialMessage), sau do goi addMessage de:
//   1. Backend chay full RAG pipeline (knowledge + medication + trace + latency).
//   2. Frontend bat dung co sendingMessage (typing 3 cham) thay vi sessionStatus.
// Tach khoi createSessionThunk thuan tuy (chi sidebar dung khi bam "Phan tich").
export const sendChatMessageThunk = createAsyncThunk<
  DiagnosticSession,
  { sessionId?: number; symptomIds: number[]; content: string },
  { rejectValue: string }
>('diagnostic/sendChatMessage', async ({ sessionId, symptomIds, content }, { rejectWithValue }) => {
  try {
    let activeSessionId = sessionId
    if (!activeSessionId) {
      const created = await diagnosticApi.createSession({ symptomIds })
      activeSessionId = created.id
    }
    await diagnosticApi.addMessage(activeSessionId, content)
    return await diagnosticApi.getSession(activeSessionId)
  } catch (err) {
    return rejectWithValue(extractError(err, 'Khong gui duoc tin nhan'))
  }
})

export const completeSessionThunk = createAsyncThunk<
  DiagnosticSession,
  number,
  { rejectValue: string }
>('diagnostic/complete', async (sessionId, { rejectWithValue }) => {
  try {
    return await diagnosticApi.completeSession(sessionId)
  } catch (err) {
    return rejectWithValue(extractError(err, 'Khong hoan tat duoc phien'))
  }
})

const diagnosticSlice = createSlice({
  name: 'diagnostic',
  initialState,
  reducers: {
    toggleSymptom(state, action: { payload: number }) {
      const id = action.payload
      const idx = state.selectedSymptomIds.indexOf(id)
      if (idx >= 0) state.selectedSymptomIds.splice(idx, 1)
      else state.selectedSymptomIds.push(id)
    },
    clearSelection(state) {
      state.selectedSymptomIds = []
    },
    resetSession(state) {
      state.currentSession = null
      state.sessionStatus = 'idle'
      state.completing = false
      state.sendingMessage = false
      state.error = null
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchSymptomsThunk.pending, (state) => {
        state.symptomsStatus = 'loading'
      })
      .addCase(fetchSymptomsThunk.fulfilled, (state, action) => {
        state.symptoms = action.payload
        state.symptomsStatus = 'success'
      })
      .addCase(fetchSymptomsThunk.rejected, (state, action) => {
        state.symptomsStatus = 'error'
        state.error = action.payload ?? 'Khong tai duoc trieu chung'
      })

      .addCase(createSessionThunk.pending, (state) => {
        state.sessionStatus = 'loading'
        state.error = null
      })
      .addCase(createSessionThunk.fulfilled, (state, action) => {
        state.currentSession = action.payload
        state.sessionStatus = 'success'
      })
      .addCase(createSessionThunk.rejected, (state, action) => {
        state.sessionStatus = 'error'
        state.error = action.payload ?? 'Khong tao duoc phien'
      })

      .addCase(fetchSessionThunk.pending, (state) => {
        state.sessionStatus = 'loading'
      })
      .addCase(fetchSessionThunk.fulfilled, (state, action) => {
        state.currentSession = action.payload
        state.sessionStatus = 'success'
      })
      .addCase(fetchSessionThunk.rejected, (state, action) => {
        state.sessionStatus = 'error'
        state.error = action.payload ?? 'Khong tai duoc phien'
      })

      .addCase(sendMessageThunk.pending, (state) => {
        state.sendingMessage = true
      })
      .addCase(sendMessageThunk.fulfilled, (state, action) => {
        state.sendingMessage = false
        state.currentSession = action.payload
      })
      .addCase(sendMessageThunk.rejected, (state, action) => {
        state.sendingMessage = false
        state.error = action.payload ?? 'Khong gui duoc tin nhan'
      })

      // sendChatMessageThunk: KHONG dung sessionStatus de tranh trigger overlay
      // "Dang phan tich" o sidebar khi user dang chat.
      .addCase(sendChatMessageThunk.pending, (state) => {
        state.sendingMessage = true
        state.error = null
      })
      .addCase(sendChatMessageThunk.fulfilled, (state, action) => {
        state.sendingMessage = false
        state.currentSession = action.payload
      })
      .addCase(sendChatMessageThunk.rejected, (state, action) => {
        state.sendingMessage = false
        state.error = action.payload ?? 'Khong gui duoc tin nhan'
      })

      .addCase(completeSessionThunk.pending, (state) => {
        state.completing = true
        state.error = null
      })
      .addCase(completeSessionThunk.fulfilled, (state, action) => {
        state.completing = false
        state.currentSession = action.payload
      })
      .addCase(completeSessionThunk.rejected, (state, action) => {
        state.completing = false
        state.error = action.payload ?? 'Khong hoan tat duoc phien'
      })

      .addMatcher(
        (action): action is { type: string } => action.type === 'auth/logout',
        () => initialState,
      )
  },
})

export const { toggleSymptom, clearSelection, resetSession } = diagnosticSlice.actions
export default diagnosticSlice.reducer
