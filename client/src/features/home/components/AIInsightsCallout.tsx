export function AIInsightsCallout() {
  return (
    <section className="py-24 px-8 bg-surface-container-low">
      <div className="max-w-5xl mx-auto bg-surface-container-lowest rounded-[2rem] p-12 border-l-4 border-l-primary shadow-sm relative overflow-hidden">
        <div className="absolute top-0 right-0 w-64 h-64 bg-primary/5 rounded-full blur-3xl -translate-y-1/2 translate-x-1/2"></div>
        <div className="flex flex-col md:flex-row gap-12 items-center">
          <div className="flex-1 space-y-6">
            <h2 className="text-3xl font-extrabold font-headline text-on-surface">Trợ lý AI của bạn đang chờ</h2>
            <p className="text-lg text-on-surface-variant leading-relaxed">
              Chỉ mất 2 phút để thực hiện khảo sát sức khỏe tổng quát. AI của chúng tôi sẽ phân tích các chỉ số và đưa ra lời khuyên y khoa cá nhân hóa dành riêng cho bạn.
            </p>
            <ul className="space-y-3">
              <li className="flex items-center gap-3 font-medium text-primary">
                <span className="material-symbols-outlined" style={{ fontVariationSettings: "'FILL' 1" }}>check_circle</span>
                Phân tích 1,000+ triệu chứng
              </li>
              <li className="flex items-center gap-3 font-medium text-primary">
                <span className="material-symbols-outlined" style={{ fontVariationSettings: "'FILL' 1" }}>check_circle</span>
                Bảo mật dữ liệu y tế tuyệt đối
              </li>
              <li className="flex items-center gap-3 font-medium text-primary">
                <span className="material-symbols-outlined" style={{ fontVariationSettings: "'FILL' 1" }}>check_circle</span>
                Kết nối trực tiếp với bác sĩ khi cần
              </li>
            </ul>
          </div>
          <div className="w-full md:w-auto">
            <button className="w-full md:w-auto bg-primary text-on-primary font-bold py-6 px-12 rounded-full text-xl shadow-xl hover:shadow-primary/30 transition-shadow">
              Bắt đầu chẩn đoán
            </button>
          </div>
        </div>
      </div>
    </section>
  );
}
