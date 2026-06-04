// =============================================================================
// ChatBox.tsx - Component vi du toi thieu dung useChat.
// Day la ban dung de ban tham khao luong - style/giao dien tuy ban tinh chinh
// theo Tailwind cua du an. Khong them tinh nang ngoai pham vi chat co ban.
// =============================================================================
import { useState } from 'react'
import { useChat } from '@/features/chat/useChat'

export default function ChatBox() {
  const { messages, send, connected, connState } = useChat()
  const [draft, setDraft] = useState('')

  const handleSend = async () => {
    if (!draft.trim()) return
    await send(draft)
    setDraft('')
  }

  return (
    <div className="flex flex-col h-[500px] border rounded-lg">
      <div className="px-4 py-2 border-b text-sm">
        Trang thai: {connected ? 'Da ket noi' : connState}
      </div>

      <div className="flex-1 overflow-y-auto p-4 space-y-2">
        {messages.map((m) => (
          <div
            key={m.id}
            className={m.senderType === 'user' ? 'text-right' : 'text-left'}
          >
            <span className="inline-block px-3 py-2 rounded-lg bg-gray-100">
              {m.content}
            </span>
          </div>
        ))}
      </div>

      <div className="flex gap-2 p-3 border-t">
        <input
          className="flex-1 border rounded px-3 py-2"
          value={draft}
          onChange={(e) => setDraft(e.target.value)}
          onKeyDown={(e) => e.key === 'Enter' && handleSend()}
          placeholder="Nhap tin nhan..."
          disabled={!connected}
        />
        <button
          className="px-4 py-2 bg-blue-600 text-white rounded disabled:opacity-50"
          onClick={handleSend}
          disabled={!connected}
        >
          Gui
        </button>
      </div>
    </div>
  )
}
