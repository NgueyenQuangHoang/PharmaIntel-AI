// =============================================================================
// NotFoundPage - Trang hiển thị khi không tìm thấy route (404)
// =============================================================================
import { Link } from 'react-router-dom';

export function NotFoundPage() {
  return (
    <div className="min-h-screen flex flex-col items-center justify-center p-6 bg-surface relative overflow-hidden">
      {/* Background decoration */}
      <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[80vw] h-[80vw] md:w-[60vw] md:h-[60vw] bg-primary/10 rounded-full blur-[120px] pointer-events-none" />

      <div className="relative z-10 flex flex-col items-center text-center">
        <div className="text-[120px] md:text-[180px] font-bold font-headline leading-none text-transparent bg-clip-text bg-gradient-to-r from-primary to-secondary drop-shadow-sm mb-4">
          404
        </div>
        
        <h1 className="text-3xl md:text-4xl font-bold text-on-surface mb-4 font-headline">
          Oops! Không tìm thấy trang
        </h1>
        
        <p className="text-on-surface-variant max-w-md mx-auto mb-10 text-lg">
          Trang bạn đang tìm kiếm có thể đã bị xóa, đổi tên, hoặc tạm thời không khả dụng.
        </p>
        
        <Link 
          to="/" 
          className="inline-flex items-center gap-2 px-8 py-3.5 bg-primary text-on-primary rounded-full font-bold shadow-lg shadow-primary/30 hover:bg-primary/90 hover:shadow-primary/40 hover:-translate-y-0.5 transition-all duration-300"
        >
          <span className="material-symbols-outlined text-[20px]">home</span>
          Về trang chủ
        </Link>
      </div>
    </div>
  );
}
