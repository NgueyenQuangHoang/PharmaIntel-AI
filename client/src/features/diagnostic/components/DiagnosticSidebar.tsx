import { useNavigate } from 'react-router-dom';
import { useState } from 'react';

export function DiagnosticSidebar() {
  const navigate = useNavigate();
  const [isAnalyzing, setIsAnalyzing] = useState(false);

  const handleAnalyzeClick = () => {
    setIsAnalyzing(true);
    // Mock API call or analysis delay
    setTimeout(() => {
      setIsAnalyzing(false);
      navigate('/diagnostic/result');
    }, 2000);
  };

  return (
    <div className="space-y-6 relative">
      {/* Loading Overlay */}
      {isAnalyzing && (
        <div className="absolute inset-0 z-50 bg-surface/60 backdrop-blur-sm rounded-xl flex flex-col items-center justify-center p-8 border border-primary/20">
          <div className="w-12 h-12 border-4 border-primary/30 border-t-primary rounded-full animate-spin mb-4"></div>
          <h3 className="text-xl font-bold font-headline text-primary mb-2">Đang phân tích dữ liệu...</h3>
          <p className="text-sm text-on-surface-variant text-center">AI đang tổng hợp các triệu chứng và tìm kiếm phác đồ phù hợp nhất với bạn.</p>
        </div>
      )}

      {/* Checklist Card */}
      <section className="bg-surface-container-lowest rounded-xl p-8 shadow-sm border border-outline-variant/15">
        <h2 className="text-xl font-bold text-on-surface mb-6 flex items-center gap-2">
          <span className="material-symbols-outlined text-secondary">checklist</span>
          Danh mục triệu chứng
        </h2>
        <div className="space-y-4">
          <div className="group">
            <p className="text-xs font-bold text-outline uppercase tracking-wider mb-3">Phổ biến</p>
            <div className="flex flex-wrap gap-2">
              <button className="px-4 py-2 rounded-full border border-outline-variant text-sm font-medium hover:border-primary hover:text-primary transition-all flex items-center gap-2 bg-surface">
                <span>Đau đầu</span>
                <span className="material-symbols-outlined text-xs">add</span>
              </button>
              <button className="px-4 py-2 rounded-full border border-primary text-primary bg-primary-fixed text-sm font-medium flex items-center gap-2">
                <span>Mệt mỏi</span>
                <span className="material-symbols-outlined text-xs">check</span>
              </button>
              <button className="px-4 py-2 rounded-full border border-outline-variant text-sm font-medium hover:border-primary hover:text-primary transition-all flex items-center gap-2 bg-surface">
                <span>Sốt nhẹ</span>
                <span className="material-symbols-outlined text-xs">add</span>
              </button>
              <button className="px-4 py-2 rounded-full border border-outline-variant text-sm font-medium hover:border-primary hover:text-primary transition-all flex items-center gap-2 bg-surface">
                <span>Ho khan</span>
                <span className="material-symbols-outlined text-xs">add</span>
              </button>
            </div>
          </div>
          <div className="group pt-4">
            <p className="text-xs font-bold text-outline uppercase tracking-wider mb-3">Tiêu hóa &amp; Khác</p>
            <div className="flex flex-wrap gap-2">
              <button className="px-4 py-2 rounded-full border border-outline-variant text-sm font-medium hover:border-primary hover:text-primary transition-all flex items-center gap-2 bg-surface">
                <span>Đau bụng</span>
                <span className="material-symbols-outlined text-xs">add</span>
              </button>
              <button className="px-4 py-2 rounded-full border border-outline-variant text-sm font-medium hover:border-primary hover:text-primary transition-all flex items-center gap-2 bg-surface">
                <span>Buồn nôn</span>
                <span className="material-symbols-outlined text-xs">add</span>
              </button>
              <button className="px-4 py-2 rounded-full border border-outline-variant text-sm font-medium hover:border-primary hover:text-primary transition-all flex items-center gap-2 bg-surface">
                <span>Chóng mặt</span>
                <span className="material-symbols-outlined text-xs">add</span>
              </button>
            </div>
          </div>
        </div>
        <div className="mt-10 pt-8 border-t border-outline-variant/15">
          <button 
            onClick={handleAnalyzeClick}
            disabled={isAnalyzing}
            className="w-full py-4 rounded-full bg-gradient-to-r from-primary to-primary-container text-on-primary font-bold text-lg shadow-lg shadow-primary/20 hover:scale-[1.02] active:scale-[0.98] transition-transform flex items-center justify-center gap-3 disabled:opacity-70 disabled:hover:scale-100"
          >
            <span className="material-symbols-outlined" style={{ fontVariationSettings: "'FILL' 1" }}>analytics</span>
            Phân tích &amp; Kê thuốc
          </button>
        </div>
      </section>
      
      {/* Insights Card */}
      <section className="bg-surface-container-low rounded-xl p-8 border-l-4 border-l-secondary">
        <h3 className="text-lg font-bold text-on-surface flex items-center gap-2 mb-3">
          <span className="material-symbols-outlined text-secondary">info</span>
          Hướng dẫn chẩn đoán
        </h3>
        <ul className="space-y-3 text-on-surface-variant text-sm leading-relaxed">
          <li className="flex gap-2">
            <span className="text-secondary font-bold">•</span>
            Mô tả chi tiết thời gian bắt đầu và mức độ đau.
          </li>
          <li className="flex gap-2">
            <span className="text-secondary font-bold">•</span>
            Ghi chú các yếu tố làm tăng hoặc giảm triệu chứng.
          </li>
          <li className="flex gap-2">
            <span className="text-secondary font-bold">•</span>
            Nếu có các dấu hiệu nguy cấp như khó thở, hãy gọi cấp cứu ngay lập tức.
          </li>
        </ul>
      </section>
      
      {/* Visual Element */}
      <div className="relative rounded-xl overflow-hidden h-40">
        <img alt="Phòng Lab hiện đại" className="w-full h-full object-cover" data-alt="abstract clinical laboratory background with soft blue lighting and blurry medical equipment in high-tech setting" src="https://lh3.googleusercontent.com/aida-public/AB6AXuB5KZyWhcFCCx-Mh_pdPYuw4bwQSFCK_A1h5HlQ69cTbYfP3akDGGp3oD-0jajMc2TXdbNgv52EVRufqLw2yhsuhj2UO0CIMjH2AWy-2aJFGhBD8HKsejRbK1QZng_MzF-9l2klt3lhjOC8YNcsos9iL_XflYYZ9U4hANYTIV4WdWvO09y3_BxVToPSQpQ_q1nIuakUIAI-fEInIoaEaHKjHhHGTKf3cjtsIlb6zFaufOojq0ZeB-TKtFrLF9KwJT1CCwRLXJa4lJo" />
        <div className="absolute inset-0 bg-gradient-to-t from-black/60 to-transparent flex items-end p-6">
          <p className="text-white text-sm font-medium italic">"Dữ liệu y tế của bạn được mã hóa và bảo mật hoàn toàn."</p>
        </div>
      </div>
    </div>
  );
}
