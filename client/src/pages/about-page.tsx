import { Link } from 'react-router-dom';

export function AboutPage() {
  return (
    <div className="min-h-screen bg-surface text-on-surface">
      {/* Hero Section */}
      <section className="relative overflow-hidden bg-gradient-to-br from-primary to-primary-container text-on-primary py-24 px-8 md:px-16">
        <div className="absolute inset-0 opacity-15 overflow-hidden">
          <div className="absolute -top-40 -right-40 w-[600px] h-[600px] bg-secondary-container rounded-full blur-[100px]"></div>
          <div className="absolute -bottom-20 -left-20 w-[400px] h-[400px] bg-tertiary rounded-full blur-[100px]"></div>
        </div>
        <div className="max-w-5xl mx-auto text-center relative z-10 space-y-6">
          <div className="inline-flex items-center px-4 py-1.5 rounded-full bg-white/10 backdrop-blur-md border border-white/20 text-secondary-fixed text-xs font-bold tracking-widest uppercase">
            Về chúng tôi • PharmaIntel AI
          </div>
          <h1 className="text-4xl md:text-6xl font-extrabold font-headline leading-tight tracking-tight">
            Tiên Phong Y Khoa <br />
            <span className="text-secondary-fixed">Kiến Tạo Bởi Trí Tuệ Nhân Tạo</span>
          </h1>
          <p className="text-lg md:text-xl text-on-primary/80 max-w-3xl mx-auto font-body leading-relaxed">
            Chúng tôi kết hợp sức mạnh của học máy tiên tiến, công nghệ RAG (Retrieval-Augmented Generation) và cơ sở dữ liệu y tế khổng lồ để mang đến giải pháp hỗ trợ chăm sóc sức khỏe cá nhân hóa, tin cậy và tức thì cho hàng triệu người Việt.
          </p>
        </div>
      </section>

      {/* Sứ mệnh & Tầm nhìn */}
      <section className="py-20 px-6 md:px-12 max-w-7xl mx-auto">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-12 items-center">
          <div className="space-y-6">
            <h2 className="text-3xl font-bold font-headline text-primary dark:text-blue-400">
              Sứ mệnh của chúng tôi
            </h2>
            <p className="text-slate-600 dark:text-slate-300 leading-relaxed font-body">
              Y tế chất lượng cao nên là một dịch vụ dễ dàng tiếp cận với bất kỳ ai, ở bất kỳ đâu và bất kỳ lúc nào. Tại PharmaIntel AI, sứ mệnh của chúng tôi là xóa nhòa khoảng cách thông tin y khoa, giúp mỗi người dân có khả năng tự theo dõi, nhận diện triệu chứng sức khỏe ban đầu một cách chính xác trước khi tiếp cận các dịch vụ y tế chuyên sâu.
            </p>
            <p className="text-slate-600 dark:text-slate-300 leading-relaxed font-body">
              Chúng tôi không thay thế bác sĩ, mà chúng tôi cung cấp một trợ lý AI đắc lực bên bạn 24/7 để đồng hành, nhắc nhở thuốc uống và định hướng chăm sóc sức khỏe an toàn nhất.
            </p>
          </div>
          <div className="relative">
            <div className="absolute -inset-2 bg-gradient-to-r from-blue-500 to-teal-500 rounded-2xl blur-xl opacity-30"></div>
            <div className="relative bg-white dark:bg-slate-800 p-8 rounded-2xl border border-slate-200/60 dark:border-slate-700/60 shadow-xl space-y-6">
              <div className="flex items-center gap-4">
                <div className="w-12 h-12 rounded-xl bg-blue-100 dark:bg-blue-900/50 flex items-center justify-center text-blue-600 dark:text-blue-400">
                  <span className="material-symbols-outlined fill-icon">visibility</span>
                </div>
                <div>
                  <h3 className="font-headline font-bold text-lg">Tầm nhìn 2030</h3>
                  <p className="text-xs text-slate-500 dark:text-slate-400">Trở thành nền tảng y tế số 1 Việt Nam</p>
                </div>
              </div>
              <blockquote className="border-l-4 border-blue-500 pl-4 italic text-slate-700 dark:text-slate-300 font-body">
                "Ứng dụng AI thông minh giúp giảm tải 40% áp lực sàng lọc ban đầu tại các cơ sở y tế cộng đồng, kiến tạo một xã hội khỏe mạnh và chủ động."
              </blockquote>
            </div>
          </div>
        </div>
      </section>

      {/* Giá trị cốt lõi */}
      <section className="bg-slate-50 dark:bg-slate-900/40 py-20 px-6 md:px-12 border-y border-slate-200/40 dark:border-slate-800/40">
        <div className="max-w-7xl mx-auto space-y-12">
          <div className="text-center space-y-4">
            <h2 className="text-3xl font-bold font-headline text-slate-900 dark:text-white">
              Cột Trụ Trải Nghiệm Số
            </h2>
            <p className="text-slate-500 dark:text-slate-400 max-w-2xl mx-auto font-body">
              Hệ sinh thái PharmaIntel AI được thiết kế xoay quanh trải nghiệm người dùng với độ chính xác và tính bảo mật cao nhất.
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            {/* Card 1 */}
            <div className="bg-white dark:bg-slate-800 p-6 rounded-2xl border border-slate-100 dark:border-slate-700/50 shadow-md hover:shadow-xl transition-all duration-300 group hover:-translate-y-1">
              <div className="w-12 h-12 rounded-xl bg-blue-50 dark:bg-blue-900/30 flex items-center justify-center text-blue-600 dark:text-blue-400 mb-6 group-hover:scale-110 transition-transform">
                <span className="material-symbols-outlined fill-icon text-2xl">neurology</span>
              </div>
              <h3 className="text-lg font-bold font-headline mb-2 text-slate-900 dark:text-white">Chẩn Đoán AI</h3>
              <p className="text-sm text-slate-500 dark:text-slate-400 leading-relaxed font-body">
                Phân tích các triệu chứng lâm sàng tức thì, đưa ra các chẩn đoán định hướng khoa học dựa trên dữ liệu chuẩn hóa quốc tế.
              </p>
            </div>

            {/* Card 2 */}
            <div className="bg-white dark:bg-slate-800 p-6 rounded-2xl border border-slate-100 dark:border-slate-700/50 shadow-md hover:shadow-xl transition-all duration-300 group hover:-translate-y-1">
              <div className="w-12 h-12 rounded-xl bg-teal-50 dark:bg-teal-900/30 flex items-center justify-center text-teal-600 dark:text-teal-400 mb-6 group-hover:scale-110 transition-transform">
                <span className="material-symbols-outlined fill-icon text-2xl">local_pharmacy</span>
              </div>
              <h3 className="text-lg font-bold font-headline mb-2 text-slate-900 dark:text-white">Tủ Thuốc Thông Minh</h3>
              <p className="text-sm text-slate-500 dark:text-slate-400 leading-relaxed font-body">
                Lưu trữ thông tin đơn thuốc cá nhân, đặt lịch nhắc nhở uống thuốc thông minh giúp bạn không bao giờ bỏ lỡ liều điều trị.
              </p>
            </div>

            {/* Card 3 */}
            <div className="bg-white dark:bg-slate-800 p-6 rounded-2xl border border-slate-100 dark:border-slate-700/50 shadow-md hover:shadow-xl transition-all duration-300 group hover:-translate-y-1">
              <div className="w-12 h-12 rounded-xl bg-indigo-50 dark:bg-indigo-900/30 flex items-center justify-center text-indigo-600 dark:text-indigo-400 mb-6 group-hover:scale-110 transition-transform">
                <span className="material-symbols-outlined fill-icon text-2xl">support_agent</span>
              </div>
              <h3 className="text-lg font-bold font-headline mb-2 text-slate-900 dark:text-white">Tư Vấn Chuyên Gia</h3>
              <p className="text-sm text-slate-500 dark:text-slate-400 leading-relaxed font-body">
                Kết nối nhanh chóng với mạng lưới dược sĩ và chuyên gia y khoa để nhận lời khuyên sức khỏe được cá nhân hóa và chuyên sâu.
              </p>
            </div>

            {/* Card 4 */}
            <div className="bg-white dark:bg-slate-800 p-6 rounded-2xl border border-slate-100 dark:border-slate-700/50 shadow-md hover:shadow-xl transition-all duration-300 group hover:-translate-y-1">
              <div className="w-12 h-12 rounded-xl bg-purple-50 dark:bg-purple-900/30 flex items-center justify-center text-purple-600 dark:text-purple-400 mb-6 group-hover:scale-110 transition-transform">
                <span className="material-symbols-outlined fill-icon text-2xl">shield</span>
              </div>
              <h3 className="text-lg font-bold font-headline mb-2 text-slate-900 dark:text-white">Bảo Mật Tối Đa</h3>
              <p className="text-sm text-slate-500 dark:text-slate-400 leading-relaxed font-body">
                Cam kết bảo mật dữ liệu hồ sơ bệnh án và nhật ký triệu chứng y tế của bạn bằng công nghệ mã hóa đầu cuối tiên tiến.
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* Điểm nhấn công nghệ RAG */}
      <section className="py-20 px-6 md:px-12 max-w-7xl mx-auto">
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-12 items-center">
          <div className="lg:col-span-5 relative">
            <div className="absolute -inset-4 bg-gradient-to-tr from-secondary/20 to-primary/20 blur-2xl rounded-3xl"></div>
            <div className="relative glass-panel rounded-3xl p-8 border border-slate-200/60 dark:border-slate-700/60 shadow-2xl space-y-6">
              <div className="text-xs font-bold text-primary dark:text-blue-400 uppercase tracking-widest">Kiến trúc hệ thống AI</div>
              <div className="space-y-4">
                <div className="flex items-center gap-4 bg-slate-100/50 dark:bg-slate-800/50 p-4 rounded-xl">
                  <div className="w-8 h-8 rounded-full bg-blue-500 text-white flex items-center justify-center font-bold text-xs">1</div>
                  <div>
                    <div className="font-bold text-sm">Nhập triệu chứng</div>
                    <div className="text-xs text-slate-500 dark:text-slate-400">Người dùng mô tả trạng thái sức khỏe</div>
                  </div>
                </div>
                <div className="flex items-center gap-4 bg-slate-100/50 dark:bg-slate-800/50 p-4 rounded-xl">
                  <div className="w-8 h-8 rounded-full bg-teal-500 text-white flex items-center justify-center font-bold text-xs">2</div>
                  <div>
                    <div className="font-bold text-sm">Truy xuất kiến thức (RAG)</div>
                    <div className="text-xs text-slate-500 dark:text-slate-400">Đối chiếu dữ liệu y khoa chuẩn lâm sàng</div>
                  </div>
                </div>
                <div className="flex items-center gap-4 bg-slate-100/50 dark:bg-slate-800/50 p-4 rounded-xl">
                  <div className="w-8 h-8 rounded-full bg-indigo-500 text-white flex items-center justify-center font-bold text-xs">3</div>
                  <div>
                    <div className="font-bold text-sm">AI Tổng hợp & Đề xuất</div>
                    <div className="text-xs text-slate-500 dark:text-slate-400">Đưa ra chẩn đoán định hướng khoa học</div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div className="lg:col-span-7 space-y-6">
            <div className="inline-flex items-center px-3 py-1 rounded-full bg-indigo-50 dark:bg-indigo-900/30 text-indigo-700 dark:text-indigo-300 text-xs font-semibold">
              CÔNG NGHỆ ĐỘT PHÁ
            </div>
            <h2 className="text-3xl font-bold font-headline text-slate-900 dark:text-white">
              Làm thế nào AI của chúng tôi đưa ra thông tin chính xác?
            </h2>
            <p className="text-slate-600 dark:text-slate-300 leading-relaxed font-body">
              Khác với các chatbot AI thông thường có thể tạo ra thông tin giả lập (ảo ảnh AI), hệ thống của chúng tôi tích hợp **công nghệ RAG (Retrieval-Augmented Generation)**. Khi người dùng đặt câu hỏi, AI sẽ thực hiện truy vấn thời gian thực trên kho kiến thức y học khổng lồ đã được kiểm duyệt bởi các bác sĩ đầu ngành.
            </p>
            <p className="text-slate-600 dark:text-slate-300 leading-relaxed font-body">
              Sau đó, mô hình ngôn ngữ lớn (LLM) sẽ đóng vai trò như một chuyên gia biên dịch, tổng hợp thông tin y khoa phức tạp thành câu trả lời dễ hiểu, an toàn và cá nhân hóa nhất cho người bệnh.
            </p>
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="bg-gradient-to-r from-blue-700 to-indigo-800 text-white py-16 px-8 text-center relative overflow-hidden">
        <div className="absolute inset-0 opacity-10">
          <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[800px] h-[400px] bg-white rounded-full blur-[120px]"></div>
        </div>
        <div className="max-w-3xl mx-auto space-y-6 relative z-10">
          <h2 className="text-3xl md:text-4xl font-bold font-headline">Sẵn sàng chăm sóc sức khỏe tốt hơn?</h2>
          <p className="text-blue-100 max-w-xl mx-auto font-body">
            Trải nghiệm công cụ chẩn đoán sức khỏe bằng AI thế hệ mới được phát triển bởi PharmaIntel AI ngay hôm nay.
          </p>
          <div className="pt-4">
            <Link
              to="/diagnostic"
              className="inline-flex items-center justify-center bg-white text-blue-700 font-bold px-8 py-4 rounded-full text-lg shadow-xl hover:scale-105 transition-all duration-200"
            >
              Chẩn đoán ngay
              <span className="material-symbols-outlined ml-2 text-lg">arrow_forward</span>
            </Link>
          </div>
        </div>
      </section>
    </div>
  );
}
