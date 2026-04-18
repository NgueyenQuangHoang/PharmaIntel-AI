export function ResultHeader() {
  return (
    <header className="mb-16">
      <div className="inline-flex items-center px-4 py-2 rounded-full bg-secondary-fixed text-on-secondary-fixed text-sm font-semibold mb-6">
        <span className="material-symbols-outlined text-sm mr-2" style={{ fontVariationSettings: "'FILL' 1" }}>auto_awesome</span>
        Phân tích bởi AI Hoàn tất
      </div>
      <h1 className="font-headline text-5xl md:text-6xl font-extrabold tracking-tighter text-on-surface mb-6 leading-tight">
        Gợi ý điều trị <span className="text-primary italic">thông minh.</span>
      </h1>
      <p className="text-on-surface-variant text-lg max-w-2xl leading-relaxed">
        Dựa trên các triệu chứng bạn đã cung cấp: <span className="font-semibold text-on-surface">đau đầu, sốt nhẹ và mệt mỏi</span>. Hệ thống đã tìm thấy 3 loại dược phẩm phù hợp để hỗ trợ giảm nhẹ tình trạng của bạn.
      </p>
    </header>
  );
}
