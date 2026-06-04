// =============================================================================
// useChat.ts - Hook quan ly mot phien chat real-time tu phia client.
//
// Trach nhiem:
//   1. Lay/tao phien (POST /api/chat/session).
//   2. Tai lich su tin nhan (GET /api/chat/{id}/messages).
//   3. Mo ket noi SignalR, JoinSession, lang nghe ReceiveMessage.
//   4. Cung cap ham send() de gui tin.
//
// Cach dung trong component:
//   const { messages, status, send, connected } = useChat()
//   ...
//   <button onClick={() => send('Xin chao duoc si')}>Gui</button>
// =============================================================================
import { useCallback, useEffect, useRef, useState } from 'react'
import type { HubConnection } from '@microsoft/signalr'
import { httpClient } from '@/services/http-client'
import {
  createChatConnection,
  joinSession,
  onReceiveMessage,
  sendMessage,
} from '@/features/chat/chat-connection'
import type { ChatMessage, ChatSession } from '@/features/chat/chat-types'

type ConnState = 'idle' | 'connecting' | 'connected' | 'error'

export function useChat() {
  const [session, setSession] = useState<ChatSession | null>(null)
  const [messages, setMessages] = useState<ChatMessage[]>([])
  const [connState, setConnState] = useState<ConnState>('idle')
  const connRef = useRef<HubConnection | null>(null)

  useEffect(() => {
    let cancelled = false
    let cleanupReceive: (() => void) | undefined

    async function start() {
      setConnState('connecting')
      try {
        // 1. Lay/tao phien
        const { data: sess } = await httpClient.post<ChatSession>('/chat/session')
        if (cancelled) return
        setSession(sess)

        // 2. Lich su tin nhan
        const { data: history } = await httpClient.get<ChatMessage[]>(
          `/chat/${sess.id}/messages`,
        )
        if (cancelled) return
        setMessages(history)

        // 3. Ket noi SignalR + join phien
        const conn = createChatConnection()
        connRef.current = conn

        // Tin nhan moi day ve real-time -> noi vao danh sach (tranh trung Id).
        cleanupReceive = onReceiveMessage(conn, (msg) => {
          setMessages((prev) =>
            prev.some((m) => m.id === msg.id) ? prev : [...prev, msg],
          )
        })

        await conn.start()
        await joinSession(conn, sess.id)
        if (cancelled) return
        setConnState('connected')
      } catch (err) {
        if (!cancelled) {
          setConnState('error')
          console.error('Chat connection error:', err)
        }
      }
    }

    start()

    return () => {
      cancelled = true
      cleanupReceive?.()
      connRef.current?.stop()
      connRef.current = null
    }
  }, [])

  const send = useCallback(
    async (content: string) => {
      const conn = connRef.current
      if (!conn || !session || !content.trim()) return
      await sendMessage(conn, session.id, content.trim())
      // Khong cap nhat state o day - server se broadcast lai qua ReceiveMessage,
      // gom ca tin cua chinh minh, nen UI dong bo voi DB.
    },
    [session],
  )

  return {
    session,
    messages,
    send,
    connected: connState === 'connected',
    connState,
  }
}
