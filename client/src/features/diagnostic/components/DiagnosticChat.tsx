export function DiagnosticChat() {
  return (
    <div className="flex flex-col h-[700px] bg-surface-container-lowest rounded-xl shadow-sm overflow-hidden border border-outline-variant/15">
      <div className="p-6 bg-gradient-to-r from-primary to-primary-container text-on-primary flex items-center justify-between">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-full bg-white/20 flex items-center justify-center backdrop-blur-md">
            <span className="material-symbols-outlined" style={{ fontVariationSettings: "'FILL' 1" }}>smart_toy</span>
          </div>
          <div>
            <p className="font-bold text-lg leading-none">PharmaIntel AI Bot</p>
            <p className="text-xs opacity-80 mt-1">Trực tuyến • Phân tích y tế thời gian thực</p>
          </div>
        </div>
        <span className="material-symbols-outlined">more_vert</span>
      </div>
      
      <div className="flex-1 overflow-y-auto p-6 space-y-6 bg-surface-container-low/30">
        {/* Bot Message */}
        <div className="flex items-start gap-3 max-w-[85%]">
          <div className="w-8 h-8 rounded-full bg-primary-fixed flex items-center justify-center shrink-0">
            <span className="material-symbols-outlined text-primary text-sm" style={{ fontVariationSettings: "'FILL' 1" }}>smart_toy</span>
          </div>
          <div className="bg-surface-container-lowest p-4 rounded-xl rounded-tl-none shadow-sm border border-outline-variant/10">
            <p className="text-on-surface leading-relaxed">Chào bạn, tôi là trợ lý y tế AI. Để tôi có thể hỗ trợ tốt nhất, vui lòng mô tả chi tiết các triệu chứng bạn đang gặp phải hoặc chọn từ danh sách bên phải.</p>
            <span className="text-[10px] text-outline mt-2 block">10:45 AM</span>
          </div>
        </div>
        
        {/* User Message */}
        <div className="flex items-start gap-3 max-w-[85%] ml-auto flex-row-reverse">
          <div className="w-8 h-8 rounded-full bg-secondary-fixed flex items-center justify-center shrink-0">
            <span className="material-symbols-outlined text-on-secondary-fixed text-sm">person</span>
          </div>
          <div className="bg-primary text-on-primary p-4 rounded-xl rounded-tr-none shadow-sm">
            <p className="leading-relaxed">Tôi cảm thấy hơi đau đầu và mệt mỏi từ sáng nay.</p>
            <span className="text-[10px] opacity-70 mt-2 block text-right">10:46 AM</span>
          </div>
        </div>
        
        {/* Bot Thinking / Typing */}
        <div className="flex items-start gap-3 max-w-[85%]">
          <div className="w-8 h-8 rounded-full bg-primary-fixed flex items-center justify-center shrink-0">
            <span className="material-symbols-outlined text-primary text-sm" style={{ fontVariationSettings: "'FILL' 1" }}>smart_toy</span>
          </div>
          <div className="bg-surface-container-lowest p-4 rounded-xl rounded-tl-none shadow-sm border border-outline-variant/10 flex gap-1 items-center">
            <div className="w-1.5 h-1.5 bg-outline rounded-full animate-bounce"></div>
            <div className="w-1.5 h-1.5 bg-outline rounded-full animate-bounce" style={{ animationDelay: '0.2s' }}></div>
            <div className="w-1.5 h-1.5 bg-outline rounded-full animate-bounce" style={{ animationDelay: '0.4s' }}></div>
          </div>
        </div>
      </div>
      
      <div className="p-4 bg-surface-container-lowest border-t border-outline-variant/15">
        <div className="flex items-center gap-2 bg-surface-container-low rounded-full px-4 py-2">
          <input className="bg-transparent border-none focus:ring-0 flex-1 text-sm outline-none" placeholder="Nhập thêm chi tiết triệu chứng..." type="text"/>
          <button className="p-2 text-primary hover:bg-primary-fixed rounded-full transition-colors">
            <span className="material-symbols-outlined">send</span>
          </button>
        </div>
        <p className="text-[11px] text-center text-outline mt-3">AI có thể đưa ra câu trả lời không chính xác. Hãy luôn tham khảo ý kiến bác sĩ.</p>
      </div>
    </div>
  );
}
