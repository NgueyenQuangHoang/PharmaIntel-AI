import { useEffect, useRef, useState, type FormEvent } from 'react';
import { useAppDispatch, useAppSelector } from '@/hooks/redux';
import { sendMessageThunk } from '@/features/diagnostic/diagnostic-slice';

const WELCOME_BOT_MESSAGE =
  'Chào bạn, tôi là trợ lý y tế AI. Để tôi có thể hỗ trợ tốt nhất, vui lòng mô tả chi tiết các triệu chứng bạn đang gặp phải hoặc chọn từ danh sách bên phải.';

function formatTime(iso: string): string {
  try {
    return new Date(iso).toLocaleTimeString('vi-VN', {
      hour: '2-digit',
      minute: '2-digit',
    });
  } catch {
    return '';
  }
}

export function DiagnosticChat() {
  const dispatch = useAppDispatch();
  const session = useAppSelector((s) => s.diagnostic.currentSession);
  const sendingMessage = useAppSelector((s) => s.diagnostic.sendingMessage);

  const [draft, setDraft] = useState('');
  const scrollRef = useRef<HTMLDivElement>(null);

  const messages = session?.messages ?? [];
  const canSend = !!session && session.status === 'in_progress';

  useEffect(() => {
    scrollRef.current?.scrollTo({
      top: scrollRef.current.scrollHeight,
      behavior: 'smooth',
    });
  }, [messages.length, sendingMessage]);

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const content = draft.trim();
    if (!content || !session || sendingMessage) return;
    setDraft('');
    try {
      await dispatch(sendMessageThunk({ sessionId: session.id, content })).unwrap();
    } catch {
      /* error in slice */
    }
  };

  return (
    <div className="flex flex-col h-[700px] bg-surface-container-lowest rounded-xl shadow-sm overflow-hidden border border-outline-variant/15">
      <div className="p-6 bg-gradient-to-r from-primary to-primary-container text-on-primary flex items-center justify-between">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-full bg-white/20 flex items-center justify-center backdrop-blur-md">
            <span
              className="material-symbols-outlined"
              style={{ fontVariationSettings: "'FILL' 1" }}
            >
              smart_toy
            </span>
          </div>
          <div>
            <p className="font-bold text-lg leading-none">PharmaIntel AI Bot</p>
            <p className="text-xs opacity-80 mt-1">
              {session
                ? `Phiên #${session.id} • ${session.status === 'in_progress' ? 'Đang phân tích' : 'Đã kết thúc'}`
                : 'Chọn triệu chứng để bắt đầu'}
            </p>
          </div>
        </div>
        <span className="material-symbols-outlined">more_vert</span>
      </div>

      <div ref={scrollRef} className="flex-1 overflow-y-auto p-6 space-y-6 bg-surface-container-low/30">
        {!session && (
          <div className="flex items-start gap-3 max-w-[85%]">
            <div className="w-8 h-8 rounded-full bg-primary-fixed flex items-center justify-center shrink-0">
              <span
                className="material-symbols-outlined text-primary text-sm"
                style={{ fontVariationSettings: "'FILL' 1" }}
              >
                smart_toy
              </span>
            </div>
            <div className="bg-surface-container-lowest p-4 rounded-xl rounded-tl-none shadow-sm border border-outline-variant/10">
              <p className="text-on-surface leading-relaxed">{WELCOME_BOT_MESSAGE}</p>
            </div>
          </div>
        )}

        {messages.map((m) => {
          const isUser = m.senderType === 'user';
          return (
            <div
              key={m.id}
              className={`flex items-start gap-3 max-w-[85%] ${isUser ? 'ml-auto flex-row-reverse' : ''}`}
            >
              <div
                className={`w-8 h-8 rounded-full flex items-center justify-center shrink-0 ${
                  isUser ? 'bg-secondary-fixed' : 'bg-primary-fixed'
                }`}
              >
                <span
                  className={`material-symbols-outlined text-sm ${
                    isUser ? 'text-on-secondary-fixed' : 'text-primary'
                  }`}
                  style={{ fontVariationSettings: "'FILL' 1" }}
                >
                  {isUser ? 'person' : 'smart_toy'}
                </span>
              </div>
              <div
                className={`p-4 rounded-xl shadow-sm ${
                  isUser
                    ? 'bg-primary text-on-primary rounded-tr-none'
                    : 'bg-surface-container-lowest border border-outline-variant/10 rounded-tl-none'
                }`}
              >
                <p className="leading-relaxed whitespace-pre-wrap">{m.content}</p>
                <span
                  className={`text-[10px] mt-2 block ${
                    isUser ? 'opacity-70 text-right' : 'text-outline'
                  }`}
                >
                  {formatTime(m.sentAt)}
                </span>
              </div>
            </div>
          );
        })}

        {sendingMessage && (
          <div className="flex items-start gap-3 max-w-[85%]">
            <div className="w-8 h-8 rounded-full bg-primary-fixed flex items-center justify-center shrink-0">
              <span
                className="material-symbols-outlined text-primary text-sm"
                style={{ fontVariationSettings: "'FILL' 1" }}
              >
                smart_toy
              </span>
            </div>
            <div className="bg-surface-container-lowest p-4 rounded-xl rounded-tl-none shadow-sm border border-outline-variant/10 flex gap-1 items-center">
              <div className="w-1.5 h-1.5 bg-outline rounded-full animate-bounce"></div>
              <div
                className="w-1.5 h-1.5 bg-outline rounded-full animate-bounce"
                style={{ animationDelay: '0.2s' }}
              ></div>
              <div
                className="w-1.5 h-1.5 bg-outline rounded-full animate-bounce"
                style={{ animationDelay: '0.4s' }}
              ></div>
            </div>
          </div>
        )}
      </div>

      <form onSubmit={handleSubmit} className="p-4 bg-surface-container-lowest border-t border-outline-variant/15">
        <div className="flex items-center gap-2 bg-surface-container-low rounded-full px-4 py-2">
          <input
            value={draft}
            onChange={(e) => setDraft(e.target.value)}
            disabled={!canSend || sendingMessage}
            className="bg-transparent border-none focus:ring-0 flex-1 text-sm outline-none disabled:opacity-50"
            placeholder={
              canSend
                ? 'Nhập thêm chi tiết triệu chứng...'
                : 'Tạo phiên bằng nút Phân tích để bắt đầu chat'
            }
            type="text"
          />
          <button
            type="submit"
            disabled={!canSend || sendingMessage || !draft.trim()}
            className="p-2 text-primary hover:bg-primary-fixed rounded-full transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
          >
            <span className="material-symbols-outlined">send</span>
          </button>
        </div>
        <p className="text-[11px] text-center text-outline mt-3">
          AI có thể đưa ra câu trả lời không chính xác. Hãy luôn tham khảo ý kiến bác sĩ.
        </p>
      </form>
    </div>
  );
}
