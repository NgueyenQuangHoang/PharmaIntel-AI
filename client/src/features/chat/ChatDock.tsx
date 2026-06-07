// =============================================================================
// ChatDock.tsx - Khung chat noi goc duoi ben phai, gan DICH DANH mot duoc si.
//
// - Khong co nut FAB toan cuc: dock chi hien khi nguoi dung bam "Chat" tren card
//   duoc si (trang Tu van). Moi duoc si la mot cuoc tro chuyen rieng.
// - Tin cua minh (senderType "user"): bong bong do, can phai.
//   Tin cua duoc si / AI (pharmacist | system): bong bong xam, can trai.
// - AI tu tra loi MOT LAN dau (xu ly o backend); sau do cho duoc si dich danh.
// =============================================================================
import { useEffect, useRef, useState } from 'react'
import { useChat } from '@/features/chat/useChat'

type Props = {
  pharmacistId: number
  pharmacistName: string
  onClose: () => void
}

export function ChatDock({ pharmacistId, pharmacistName, onClose }: Props) {
  const { messages, send, connected, connState } = useChat(pharmacistId)
  const [draft, setDraft] = useState('')
  const scrollRef = useRef<HTMLDivElement>(null)

  // Tu cuon xuong cuoi khi co tin moi.
  useEffect(() => {
    scrollRef.current?.scrollTo({ top: scrollRef.current.scrollHeight, behavior: 'smooth' })
  }, [messages.length])

  const handleSend = async () => {
    const text = draft.trim()
    if (!text) return
    setDraft('')
    await send(text)
  }

  return (
    <div className="fixed bottom-5 right-5 z-50 w-[360px] max-w-[calc(100vw-2.5rem)]">
      <div className="flex flex-col h-[520px] max-h-[calc(100vh-2.5rem)] rounded-2xl bg-zinc-900 text-zinc-100 shadow-2xl border border-zinc-800 overflow-hidden">
        {/* Header */}
        <div className="flex items-center gap-3 px-4 py-3 bg-zinc-800/60 border-b border-zinc-800">
          <div className="w-9 h-9 rounded-full bg-zinc-700 flex items-center justify-center">
            <span className="material-symbols-outlined text-[20px]">support_agent</span>
          </div>
          <div className="flex-1 min-w-0">
            <p className="font-semibold text-sm flex items-center gap-1.5 truncate">
              {pharmacistName}
              <span
                className={`inline-block w-2 h-2 rounded-full ${connected ? 'bg-green-500' : 'bg-zinc-500'}`}
                title={connected ? 'Trực tuyến' : connState}
              />
            </p>
            <p className="text-xs text-zinc-400 truncate">Thường trả lời sau vài phút</p>
          </div>
          <button
            onClick={onClose}
            className="p-1.5 rounded-lg hover:bg-zinc-700/60 transition-colors"
            title="Đóng"
          >
            <span className="material-symbols-outlined text-[20px]">close</span>
          </button>
        </div>

        {/* Body */}
        <div ref={scrollRef} className="flex-1 overflow-y-auto px-4 py-3 space-y-3">
          {messages.length === 0 && (
            <div className="text-center text-zinc-400 mt-6 px-4">
              <div className="w-12 h-12 mx-auto mb-3 rounded-full bg-zinc-800 flex items-center justify-center">
                <span className="material-symbols-outlined">waving_hand</span>
              </div>
              <p className="font-semibold text-zinc-200">Xin chào</p>
              <p className="text-sm mt-1">Hãy nhập câu hỏi, trợ lý sẽ phản hồi ngay và dược sĩ sẽ tiếp nhận sau.</p>
            </div>
          )}

          {messages.map((m) => {
            const mine = m.senderType === 'user'
            return (
              <div key={m.id} className={`flex items-end gap-2 ${mine ? 'justify-end' : 'justify-start'}`}>
                {!mine && (
                  <div className="w-7 h-7 rounded-full bg-zinc-700 flex items-center justify-center flex-shrink-0">
                    <span className="material-symbols-outlined text-[16px]">support_agent</span>
                  </div>
                )}
                <div
                  className={`max-w-[75%] px-3.5 py-2 rounded-2xl text-sm break-words ${
                    mine ? 'bg-red-500 text-white rounded-br-md' : 'bg-zinc-800 text-zinc-100 rounded-bl-md'
                  }`}
                >
                  {m.content}
                </div>
              </div>
            )
          })}
        </div>

        {/* Input */}
        <div className="p-3 border-t border-zinc-800 bg-zinc-900">
          <div className="flex items-center gap-2 rounded-full bg-zinc-800 border border-zinc-700 px-4 py-1 focus-within:border-red-500 transition-colors">
            <input
              className="flex-1 bg-transparent py-2 text-sm placeholder:text-zinc-500 focus:outline-none disabled:opacity-50"
              value={draft}
              onChange={(e) => setDraft(e.target.value)}
              onKeyDown={(e) => e.key === 'Enter' && handleSend()}
              placeholder="Gõ tin nhắn của bạn"
              disabled={!connected}
            />
            <button
              onClick={handleSend}
              disabled={!connected || !draft.trim()}
              className="text-red-500 hover:text-red-400 disabled:text-zinc-600 transition-colors"
              title="Gửi"
            >
              <span className="material-symbols-outlined">send</span>
            </button>
          </div>
          {!connected && connState === 'error' && (
            <p className="text-xs text-red-400 mt-2 px-2">Không kết nối được, vui lòng thử lại.</p>
          )}
        </div>
      </div>
    </div>
  )
}
