// =============================================================================
// PharmacistChatPanel - Tab "Tin nhan" trong /pharmacist.
// Chuc nang: duoc si xem danh sach phien (Hang cho / Da nhan), mo mot phien de
// chat real-time. Khi duoc si gui tin vao phien dang cho -> backend tu lat phien
// sang "open" + gan duoc si -> AI ngung tra loi (tiep quan).
//
// Tai su dung: chat-connection.ts (createChatConnection/joinSession/onReceiveMessage/
// sendMessage) + GET /chat/{id}/messages (quyen truy cap da xu ly o IChatService).
// =============================================================================
import { useCallback, useEffect, useRef, useState } from 'react'
import axios from 'axios'
import type { HubConnection } from '@microsoft/signalr'
import { httpClient } from '@/services/http-client'
import {
  createChatConnection,
  joinSession,
  onReceiveMessage,
  sendMessage,
} from '@/features/chat/chat-connection'
import type { ChatMessage, ChatSessionListItem } from '@/features/chat/chat-types'

type SubTab = 'waiting' | 'open'

const SUB_TAB_LABELS: Record<SubTab, string> = {
  waiting: 'Hàng chờ',
  open: 'Đã nhận',
}

function extractApiError(err: unknown, fallback: string) {
  if (axios.isAxiosError(err)) {
    const data = err.response?.data as { title?: string; detail?: string; message?: string } | undefined
    return data?.detail ?? data?.title ?? data?.message ?? err.message ?? fallback
  }
  return fallback
}

export function PharmacistChatPanel() {
  const [subTab, setSubTab] = useState<SubTab>('waiting')
  const [sessions, setSessions] = useState<ChatSessionListItem[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const [activeId, setActiveId] = useState<number | null>(null)
  const [messages, setMessages] = useState<ChatMessage[]>([])
  const [draft, setDraft] = useState('')
  const [connected, setConnected] = useState(false)
  const connRef = useRef<HubConnection | null>(null)
  const scrollRef = useRef<HTMLDivElement>(null)

  const loadSessions = useCallback(async (tab: SubTab) => {
    setLoading(true)
    setError(null)
    try {
      const { data } = await httpClient.get<ChatSessionListItem[]>('/pharmacist/chat/sessions', {
        params: { status: tab },
      })
      setSessions(data)
    } catch (err) {
      setError(extractApiError(err, 'Không tải được danh sách phiên chat'))
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    void loadSessions(subTab)
  }, [subTab, loadSessions])

  // Mo ket noi real-time cho phien dang chon. Doi phien -> dong cu, mo moi.
  useEffect(() => {
    if (activeId === null) return
    let cancelled = false
    let cleanupReceive: (() => void) | undefined
    setConnected(false)
    setMessages([])

    async function open(sessionId: number) {
      try {
        const { data: history } = await httpClient.get<ChatMessage[]>(`/chat/${sessionId}/messages`)
        if (cancelled) return
        setMessages(history)

        const conn = createChatConnection()
        connRef.current = conn
        cleanupReceive = onReceiveMessage(conn, (msg) => {
          setMessages((prev) => (prev.some((m) => m.id === msg.id) ? prev : [...prev, msg]))
        })
        await conn.start()
        await joinSession(conn, sessionId)
        if (cancelled) return
        setConnected(true)
      } catch (err) {
        if (!cancelled) setError(extractApiError(err, 'Không kết nối được phiên chat'))
      }
    }

    void open(activeId)

    return () => {
      cancelled = true
      cleanupReceive?.()
      connRef.current?.stop()
      connRef.current = null
    }
  }, [activeId])

  // Tu cuon xuong cuoi khi co tin moi.
  useEffect(() => {
    scrollRef.current?.scrollTo({ top: scrollRef.current.scrollHeight, behavior: 'smooth' })
  }, [messages.length])

  async function handleSend() {
    const text = draft.trim()
    const conn = connRef.current
    if (!text || !conn || activeId === null) return
    setDraft('')
    try {
      await sendMessage(conn, activeId, text)
      // Gui tin dau tien vao phien "waiting" -> phien thanh "open"; lam moi danh sach.
      if (subTab === 'waiting') void loadSessions('waiting')
    } catch (err) {
      setError(extractApiError(err, 'Gửi tin nhắn thất bại'))
    }
  }

  return (
    <div className="grid grid-cols-1 lg:grid-cols-[320px_1fr] gap-5 min-h-[520px]">
      {/* Cot trai: danh sach phien */}
      <div className="flex flex-col">
        <div className="mb-4 inline-flex p-1 bg-surface-container rounded-2xl border border-outline-variant/30 self-start">
          {(Object.keys(SUB_TAB_LABELS) as SubTab[]).map((t) => (
            <button
              key={t}
              type="button"
              onClick={() => setSubTab(t)}
              className={`px-4 py-1.5 font-semibold text-sm rounded-xl transition-colors ${
                subTab === t ? 'bg-primary text-on-primary shadow-sm' : 'text-on-surface-variant hover:text-on-surface'
              }`}
            >
              {SUB_TAB_LABELS[t]}
            </button>
          ))}
        </div>

        {error && (
          <div className="mb-3 p-3 rounded-xl bg-error-container text-on-error-container text-sm font-medium">
            {error}
          </div>
        )}

        {loading ? (
          <div className="p-6 rounded-xl bg-surface-container-low text-on-surface-variant">Đang tải...</div>
        ) : sessions.length === 0 ? (
          <div className="p-8 rounded-2xl bg-surface-container-low text-center border border-outline-variant/20">
            <span className="material-symbols-outlined text-4xl text-outline mb-2">forum</span>
            <p className="text-on-surface-variant text-sm">
              {subTab === 'waiting' ? 'Không có phiên nào đang chờ.' : 'Bạn chưa nhận phiên nào.'}
            </p>
          </div>
        ) : (
          <div className="space-y-2 overflow-y-auto pr-1">
            {sessions.map((s) => (
              <button
                key={s.id}
                type="button"
                onClick={() => setActiveId(s.id)}
                className={`w-full text-left p-3 rounded-xl border transition-colors ${
                  activeId === s.id
                    ? 'bg-primary-container/40 border-primary/40'
                    : 'bg-surface-container-lowest border-outline-variant/20 hover:bg-surface-container-low'
                }`}
              >
                <div className="flex items-center justify-between gap-2">
                  <span className="font-semibold text-sm truncate">{s.userFullName || `Người dùng #${s.userId}`}</span>
                  <span className="text-[10px] uppercase font-bold text-on-surface-variant flex-shrink-0">{s.status}</span>
                </div>
                <p className="text-xs text-on-surface-variant truncate mt-0.5">{s.lastMessage ?? '(chưa có tin nhắn)'}</p>
              </button>
            ))}
          </div>
        )}
      </div>

      {/* Cot phai: khung hoi thoai */}
      <div className="flex flex-col rounded-2xl border border-outline-variant/20 bg-surface-container-lowest overflow-hidden">
        {activeId === null ? (
          <div className="flex-1 flex flex-col items-center justify-center text-on-surface-variant p-8">
            <span className="material-symbols-outlined text-5xl text-outline mb-3">chat</span>
            <p>Chọn một phiên để bắt đầu trả lời.</p>
          </div>
        ) : (
          <>
            <div className="px-4 py-3 border-b border-outline-variant/20 bg-surface-container/30 flex items-center gap-2">
              <span
                className={`inline-block w-2 h-2 rounded-full ${connected ? 'bg-green-500' : 'bg-outline'}`}
              />
              <span className="font-semibold text-sm">Phiên #{activeId}</span>
            </div>

            <div ref={scrollRef} className="flex-1 overflow-y-auto p-4 space-y-3">
              {messages.map((m) => {
                // Goc nhin duoc si: tin cua duoc si (pharmacist) ben phai; user/AI ben trai.
                const mine = m.senderType === 'pharmacist'
                return (
                  <div key={m.id} className={`flex ${mine ? 'justify-end' : 'justify-start'}`}>
                    <div
                      className={`max-w-[75%] px-3.5 py-2 rounded-2xl text-sm break-words ${
                        mine
                          ? 'bg-primary text-on-primary rounded-br-md'
                          : m.senderType === 'system'
                            ? 'bg-secondary-container text-on-secondary-container rounded-bl-md'
                            : 'bg-surface-container-high text-on-surface rounded-bl-md'
                      }`}
                    >
                      {m.senderType === 'system' && (
                        <span className="block text-[10px] font-bold opacity-70 mb-0.5">Trợ lý AI</span>
                      )}
                      {m.content}
                    </div>
                  </div>
                )
              })}
            </div>

            <div className="p-3 border-t border-outline-variant/20">
              <div className="flex items-center gap-2 rounded-full bg-surface-container border border-outline-variant/40 px-4 focus-within:border-primary transition-colors">
                <input
                  className="flex-1 bg-transparent py-2.5 text-sm focus:outline-none disabled:opacity-50"
                  value={draft}
                  onChange={(e) => setDraft(e.target.value)}
                  onKeyDown={(e) => e.key === 'Enter' && handleSend()}
                  placeholder="Nhập câu trả lời..."
                  disabled={!connected}
                />
                <button
                  onClick={handleSend}
                  disabled={!connected || !draft.trim()}
                  className="text-primary hover:text-primary/80 disabled:text-outline transition-colors"
                  title="Gửi"
                >
                  <span className="material-symbols-outlined">send</span>
                </button>
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  )
}
