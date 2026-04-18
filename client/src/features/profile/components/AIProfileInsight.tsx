export function AIProfileInsight() {
  return (
    <section className="mt-12 bg-surface-container-lowest rounded-xl p-8 border border-outline-variant/15 flex flex-col md:flex-row items-center gap-8 relative overflow-hidden shadow-sm">
      <div className="absolute top-0 left-0 w-1 h-full bg-gradient-to-b from-secondary to-primary"></div>
      <div className="w-20 h-20 bg-primary-fixed rounded-2xl flex items-center justify-center shrink-0 shadow-inner">
        <span className="material-symbols-outlined text-4xl text-primary font-light">psychology</span>
      </div>
      <div className="flex-grow">
        <h3 className="text-xl font-bold mb-2">Phân tích Sức khỏe Thông minh</h3>
        <p className="text-on-surface-variant text-sm leading-relaxed max-w-2xl">
          Dựa trên lịch sử chẩn đoán và chỉ số sức khỏe gần đây, hệ thống nhận thấy sự cải thiện rõ rệt về huyết áp. Hãy tiếp tục duy trì chế độ ăn giảm muối và uống thuốc Amoxicillin đúng giờ để dứt điểm đợt viêm họng này.
        </p>
      </div>
      <button className="px-8 py-3 border-2 border-primary text-primary font-bold rounded-full hover:bg-primary hover:text-on-primary transition-all whitespace-nowrap active:scale-95 shadow-sm hover:shadow-md">
        Xem chi tiết phân tích
      </button>
    </section>
  );
}
