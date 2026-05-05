// =============================================================================
// Diagnostic API
// =============================================================================
import { httpClient } from '@/services/http-client'
import type {
  CreateDiagnosticSessionRequest,
  DiagnosticMessage,
  DiagnosticResult,
  DiagnosticSession,
  Symptom,
} from './types'

export const diagnosticApi = {
  listSymptoms: async (groupName?: string): Promise<Symptom[]> => {
    const res = await httpClient.get<Symptom[]>('/symptoms', {
      params: groupName ? { groupName } : undefined,
    })
    return res.data
  },
  listMySessions: async (params?: { page?: number; pageSize?: number; status?: string }) => {
    const res = await httpClient.get('/diagnostics/sessions/my', { params })
    return res.data
  },
  createSession: async (body: CreateDiagnosticSessionRequest): Promise<DiagnosticSession> => {
    const res = await httpClient.post<DiagnosticSession>('/diagnostics/sessions', body)
    return res.data
  },
  getSession: async (id: number): Promise<DiagnosticSession> => {
    const res = await httpClient.get<DiagnosticSession>(`/diagnostics/sessions/${id}`)
    return res.data
  },
  addMessage: async (sessionId: number, content: string): Promise<DiagnosticMessage> => {
    const res = await httpClient.post<DiagnosticMessage>(
      `/diagnostics/sessions/${sessionId}/messages`,
      { content },
    )
    return res.data
  },
  completeSession: async (id: number): Promise<DiagnosticSession> => {
    const res = await httpClient.post<DiagnosticSession>(
      `/diagnostics/sessions/${id}/complete`,
      {},
    )
    return res.data
  },
  getResult: async (id: number): Promise<DiagnosticResult> => {
    const res = await httpClient.get<DiagnosticResult>(`/diagnostics/results/${id}`)
    return res.data
  },
}
