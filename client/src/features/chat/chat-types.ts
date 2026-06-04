// =============================================================================
// Types: chat real-time benh nhan <-> duoc si.
// Khop voi ChatMessageDto / ChatSessionDto ben server.
// =============================================================================

export type ChatSenderType = 'user' | 'pharmacist' | 'system'
export type ChatSessionStatus = 'open' | 'waiting' | 'closed' | 'cancelled'

export interface ChatMessage {
  id: number
  sessionId: number
  senderType: ChatSenderType
  senderUserId: number | null
  senderPharmacistId: number | null
  content: string
  sentAt: string // ISO
}

export interface ChatSession {
  id: number
  userId: number
  pharmacistId: number | null
  status: ChatSessionStatus
  startedAt: string
  closedAt: string | null
}
