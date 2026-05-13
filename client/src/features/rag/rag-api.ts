import { httpClient } from '@/services/http-client'

export type KnowledgeDocument = {
  id: number
  title: string
  sourceType: string
  sourceUrl?: string | null
  description?: string | null
  isActive: boolean
  chunkCount: number
  createdAt: string
  updatedAt: string
}

export type IngestKnowledgeRequest = {
  title: string
  sourceType: string
  content: string
  sourceUrl?: string | null
}

export type UpdateKnowledgeDocumentRequest = {
  title: string
  sourceType: string
  sourceUrl?: string | null
  description?: string | null
  content: string
  isActive: boolean
}

export type AiFeedback = {
  id: number
  userId: number
  diagnosticSessionId?: number | null
  diagnosticMessageId?: number | null
  ragTraceId?: number | null
  rating: 'thumbs_up' | 'thumbs_down'
  reasonType?: string | null
  comment?: string | null
  isReviewed: boolean
  adminNote?: string | null
  createdAt: string
  reviewedAt?: string | null
}

export type RagDashboard = {
  totalTraces: number
  totalFeedbacks: number
  negativeFeedbacks: number
  unreviewedNegativeFeedbacks: number
  negativeFeedbackRate: number
  noContextResponses: number
  medicationContextResponses: number
  knowledgeContextResponses: number
  avgRetrievalLatencyMs?: number
  avgGenerationLatencyMs?: number
  avgTotalLatencyMs?: number
  p95TotalLatencyMs?: number
  failedJobs?: number
  queuedJobs?: number
  embeddingCacheSize?: number
}

export const ragApi = {
  listKnowledge: async (): Promise<KnowledgeDocument[]> => {
    const res = await httpClient.get<KnowledgeDocument[]>('/admin/knowledge')
    return res.data
  },

  getKnowledge: async (id: number): Promise<KnowledgeDocument> => {
    const res = await httpClient.get<KnowledgeDocument>(`/admin/knowledge/${id}`)
    return res.data
  },

  ingestKnowledge: async (body: IngestKnowledgeRequest): Promise<{ documentId: number }> => {
    const res = await httpClient.post<{ documentId: number }>('/admin/knowledge/ingest', body)
    return res.data
  },

  updateKnowledge: async (
    id: number,
    body: UpdateKnowledgeDocumentRequest,
  ): Promise<KnowledgeDocument> => {
    const res = await httpClient.put<KnowledgeDocument>(`/admin/knowledge/${id}`, body)
    return res.data
  },

  reindexKnowledge: async (id: number): Promise<void> => {
    await httpClient.post(`/admin/knowledge/${id}/reindex`)
  },

  deleteKnowledge: async (id: number): Promise<void> => {
    await httpClient.delete(`/admin/knowledge/${id}`)
  },

  listFeedback: async (params?: {
    isReviewed?: boolean
    rating?: 'thumbs_up' | 'thumbs_down'
  }): Promise<AiFeedback[]> => {
    const res = await httpClient.get<AiFeedback[]>('/admin/ai-feedback', { params })
    return res.data
  },

  reviewFeedback: async (id: number, adminNote?: string): Promise<AiFeedback> => {
    const res = await httpClient.post<AiFeedback>(`/admin/ai-feedback/${id}/review`, {
      adminNote,
    })
    return res.data
  },

  getDashboard: async (): Promise<RagDashboard> => {
    const res = await httpClient.get<RagDashboard>('/admin/rag-dashboard')
    return res.data
  },

  createFeedback: async (body: {
    diagnosticSessionId?: number | null
    diagnosticMessageId?: number | null
    ragTraceId?: number | null
    rating: 'thumbs_up' | 'thumbs_down'
    reasonType?: string | null
    comment?: string | null
  }): Promise<AiFeedback> => {
    const res = await httpClient.post<AiFeedback>('/ai-feedback', body)
    return res.data
  },
}
