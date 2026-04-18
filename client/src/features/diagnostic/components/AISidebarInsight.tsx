export function AISidebarInsight() {
  return (
    <aside className="space-y-8">
      {/* AI Insight Card */}
      <section className="bg-surface-container-lowest p-8 rounded-xl border border-outline-variant/15 border-l-4 border-l-primary shadow-sm">
        <h3 className="font-headline text-xl font-bold text-on-surface mb-4 flex items-center">
          <span className="material-symbols-outlined mr-2 text-primary" style={{ fontVariationSettings: "'FILL' 1" }}>lightbulb</span>
          Lời khuyên từ AI
        </h3>
        <div className="space-y-4">
          <p className="text-on-surface-variant leading-relaxed">
            Nhiệt độ cơ thể hiện tại của bạn cho thấy dấu hiệu của <span className="text-on-surface font-semibold italic">Sốt nhẹ</span>. 
          </p>
          <ul className="space-y-3">
            <li className="flex items-start">
              <span className="material-symbols-outlined text-secondary text-lg mr-3 mt-1" style={{ fontVariationSettings: "'FILL' 1" }}>water_drop</span>
              <span className="text-sm text-on-surface-variant">Uống thêm ít nhất 500ml nước mỗi 2 giờ.</span>
            </li>
            <li className="flex items-start">
              <span className="material-symbols-outlined text-secondary text-lg mr-3 mt-1">bed</span>
              <span className="text-sm text-on-surface-variant">Nghỉ ngơi hoàn toàn trong môi trường thoáng mát.</span>
            </li>
            <li className="flex items-start">
              <span className="material-symbols-outlined text-secondary text-lg mr-3 mt-1">monitor_heart</span>
              <span className="text-sm text-on-surface-variant">Theo dõi nhiệt độ mỗi 4 giờ một lần.</span>
            </li>
          </ul>
        </div>
      </section>

      {/* Medical Disclaimer Section */}
      <section className="bg-error-container/20 p-8 rounded-xl border border-error/10">
        <div className="flex items-center text-error font-bold mb-4">
          <span className="material-symbols-outlined mr-2">warning</span>
          Miễn trừ trách nhiệm y tế
        </div>
        <div className="text-xs text-on-surface-variant space-y-3 leading-relaxed">
          <p>Thông tin được cung cấp bởi <span className="font-bold">PharmaIntel AI</span> chỉ mang tính chất tham khảo và không thay thế cho lời khuyên, chẩn đoán hoặc điều trị chuyên môn từ bác sĩ y khoa.</p>
          <p>Luôn tìm kiếm lời khuyên từ bác sĩ của bạn hoặc nhà cung cấp dịch vụ y tế có trình độ chuyên môn khác nếu bạn có bất kỳ câu hỏi nào liên quan đến tình trạng sức khỏe.</p>
          <p className="font-semibold text-on-surface">Trong trường hợp khẩn cấp, hãy liên hệ ngay với trung tâm cấp cứu gần nhất (115).</p>
        </div>
      </section>

      {/* Support Card */}
      <section className="bg-surface-container-high p-8 rounded-xl text-center">
        <div className="w-16 h-16 bg-white rounded-full flex items-center justify-center mx-auto mb-4 shadow-sm">
          <span className="material-symbols-outlined text-primary text-3xl">support_agent</span>
        </div>
        <h4 className="font-headline font-bold text-on-surface mb-2">Cần tư vấn trực tiếp?</h4>
        <p className="text-sm text-on-surface-variant mb-6">Kết nối ngay với dược sĩ chuyên môn để được tư vấn chi tiết hơn.</p>
        <button className="w-full py-2 px-6 rounded-full border-2 border-primary text-primary font-bold hover:bg-primary hover:text-on-primary transition-all">
          Trò chuyện với Dược sĩ
        </button>
      </section>
    </aside>
  );
}
