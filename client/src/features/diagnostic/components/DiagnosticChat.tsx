import { useEffect, useMemo, useRef, useState, type FormEvent } from 'react';
import { useAppDispatch, useAppSelector } from '@/hooks/redux';
import { sendChatMessageThunk } from '@/features/diagnostic/diagnostic-slice';
import { ragApi } from '@/features/rag/rag-api';

const WELCOME_BOT_MESSAGE =
  'Chào bạn, tôi là trợ lý y tế AI. Để tôi có thể hỗ trợ tốt nhất, vui lòng mô tả chi tiết các triệu chứng bạn đang gặp phải hoặc chọn từ danh sách bên phải.';

type LocalChatMessage = {
  id: string | number;
  senderType: 'user' | 'ai' | 'system';
  content: string;
  sentAt: string;
  pending?: boolean;
};

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
  const selectedSymptomIds = useAppSelector((s) => s.diagnostic.selectedSymptomIds);

  const [draft, setDraft] = useState('');
  const [optimisticMessages, setOptimisticMessages] = useState<LocalChatMessage[]>([]);
  const scrollRef = useRef<HTMLDivElement>(null);

  const [feedbackTarget, setFeedbackTarget] = useState<{
    messageId: number;
    rating: 'thumbs_up' | 'thumbs_down';
  } | null>(null);

  const [feedbackReason, setFeedbackReason] = useState('not_helpful');
  const [feedbackComment, setFeedbackComment] = useState('');

  const submitFeedback = async () => {
    if (!feedbackTarget || !session) return;

    await ragApi.createFeedback({
      diagnosticSessionId: session.id,
      diagnosticMessageId: feedbackTarget.messageId,
      rating: feedbackTarget.rating,
      reasonType: feedbackTarget.rating === 'thumbs_down' ? feedbackReason : null,
      comment: feedbackComment.trim() || null,
    });

    setFeedbackTarget(null);
    setFeedbackReason('not_helpful');
    setFeedbackComment('');
  };

  const serverMessages = session?.messages ?? [];

  // Goi giu user bubble optimistic cho den khi server tra ve message that
  // (match theo content). Sau khi server da co message do, drop optimistic.
  const visibleMessages = useMemo<LocalChatMessage[]>(() => {
    const merged: LocalChatMessage[] = serverMessages.map((m) => ({
      id: m.id,
      senderType: m.senderType as LocalChatMessage['senderType'],
      content: m.content,
      sentAt: m.sentAt,
    }));
    const serverUserContents = new Set(
      serverMessages.filter((m) => m.senderType === 'user').map((m) => m.content),
    );
    optimisticMessages.forEach((opt) => {
      if (!serverUserContents.has(opt.content)) merged.push(opt);
    });
    return merged;
  }, [serverMessages, optimisticMessages]);

  // Sau khi server tra ve, drop cac optimistic da match.
  useEffect(() => {
    if (optimisticMessages.length === 0) return;
    const serverUserContents = new Set(
      serverMessages.filter((m) => m.senderType === 'user').map((m) => m.content),
    );
    const remaining = optimisticMessages.filter((m) => !serverUserContents.has(m.content));
    if (remaining.length !== optimisticMessages.length) {
      setOptimisticMessages(remaining);
    }
  }, [serverMessages, optimisticMessages]);

  // Cho phep gui khi: dang co session in_progress, HOAC chua co session (auto-tao).
  const canSend = !session || session.status === 'in_progress';

  useEffect(() => {
    scrollRef.current?.scrollTo({
      top: scrollRef.current.scrollHeight,
      behavior: 'smooth',
    });
  }, [visibleMessages.length, sendingMessage]);

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const content = draft.trim();
    if (!content || sendingMessage) return;

    setDraft('');
    const optimisticId = `local-${Date.now()}`;
    setOptimisticMessages((prev) => [
      ...prev,
      {
        id: optimisticId,
        senderType: 'user',
        content,
        sentAt: new Date().toISOString(),
        pending: true,
      },
    ]);

    try {
      await dispatch(
        sendChatMessageThunk({
          sessionId: session?.id,
          symptomIds: selectedSymptomIds,
          content,
        }),
      ).unwrap();
    } catch {
      // Giu lai bubble user va danh dau loi de user thay duoc.
      setOptimisticMessages((prev) =>
        prev.map((m) =>
          m.id === optimisticId
            ? {
                ...m,
                content: `${m.content}\n\n(Không gửi được. Vui lòng thử lại.)`,
                pending: false,
              }
            : m,
        ),
      );
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
                ? `Phiên #${session.id} • ${session.status === 'in_progress' ? 'Đang trò chuyện' : 'Đã kết thúc'}`
                : 'Chọn triệu chứng để bắt đầu'}
            </p>
          </div>
        </div>
        <span className="material-symbols-outlined">more_vert</span>
      </div>

      <div ref={scrollRef} className="flex-1 overflow-y-auto p-6 space-y-6 bg-surface-container-low/30">
        {!session && optimisticMessages.length === 0 && (
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

        {visibleMessages.map((m) => {
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
                } ${m.pending ? 'opacity-70' : ''}`}
              >
                <p className="leading-relaxed whitespace-pre-wrap">{m.content}</p>
                <span
                  className={`text-[10px] mt-2 block ${
                    isUser ? 'opacity-70 text-right' : 'text-outline'
                  }`}
                >
                  {formatTime(m.sentAt)}
                </span>
                {!isUser && (
                  <div className="mt-2 flex items-center gap-2">
                    <button
                      type="button"
                      onClick={() =>
                        setFeedbackTarget({
                          messageId: Number(m.id),
                          rating: 'thumbs_up',
                        })
                      }
                      className="text-xs rounded-full border border-outline-variant/40 px-2 py-1 hover:bg-primary-container/30"
                    >
                      👍 Hữu ích
                    </button>
                    <button
                      type="button"
                      onClick={() =>
                        setFeedbackTarget({
                          messageId: Number(m.id),
                          rating: 'thumbs_down',
                        })
                      }
                      className="text-xs rounded-full border border-outline-variant/40 px-2 py-1 hover:bg-error-container/30"
                    >
                      👎 Báo lỗi
                    </button>
                  </div>
                )}
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
              !session
                ? 'Mô tả triệu chứng để bắt đầu chat...'
                : canSend
                  ? 'Nhập thêm chi tiết triệu chứng...'
                  : 'Phiên đã kết thúc'
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

      {feedbackTarget && (
        <div className="fixed inset-0 z-50 bg-black/40 flex items-center justify-center p-4">
          <div className="w-full max-w-md rounded-2xl bg-surface p-6 shadow-xl border border-outline-variant/40">
            <h3 className="font-bold text-lg mb-2">
              {feedbackTarget.rating === 'thumbs_up'
                ? 'Cảm ơn bạn đã đánh giá'
                : 'Bạn gặp vấn đề gì với câu trả lời này?'}
            </h3>

            {feedbackTarget.rating === 'thumbs_down' && (
              <>
                <label className="block text-sm font-semibold mb-1">Lý do</label>
                <select
                  value={feedbackReason}
                  onChange={(e) => setFeedbackReason(e.target.value)}
                  className="w-full rounded-xl border border-outline-variant/40 bg-surface-container-low px-4 py-2 mb-3 text-sm"
                >
                  <option value="wrong_medication">Gợi ý thuốc sai</option>
                  <option value="unsafe_advice">Lời khuyên không an toàn</option>
                  <option value="not_helpful">Không hữu ích</option>
                  <option value="hallucination">Bịa thông tin</option>
                  <option value="other">Khác</option>
                </select>
              </>
            )}

            <label className="block text-sm font-semibold mb-1">Ghi chú thêm</label>
            <textarea
              value={feedbackComment}
              onChange={(e) => setFeedbackComment(e.target.value)}
              className="w-full min-h-24 rounded-xl border border-outline-variant/40 bg-surface-container-low px-4 py-3 text-sm"
              placeholder="Mô tả ngắn để admin cải thiện hệ thống..."
            />

            <div className="mt-4 flex justify-end gap-2">
              <button
                type="button"
                onClick={() => setFeedbackTarget(null)}
                className="rounded-full border border-outline-variant px-4 py-2 text-sm font-semibold"
              >
                Hủy
              </button>
              <button
                type="button"
                onClick={submitFeedback}
                className="rounded-full bg-primary text-on-primary px-4 py-2 text-sm font-semibold"
              >
                Gửi feedback
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
