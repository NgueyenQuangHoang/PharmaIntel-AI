export function HeroSection() {
  return (
    <section className="relative min-h-[870px] flex items-center overflow-hidden bg-gradient-to-br from-primary to-primary-container px-8 py-20">
      <div className="absolute inset-0 opacity-20 overflow-hidden">
        <div className="absolute top-0 right-0 w-[800px] h-[800px] bg-secondary-container rounded-full blur-[120px] -translate-y-1/2 translate-x-1/3"></div>
        <div className="absolute bottom-0 left-0 w-[600px] h-[600px] bg-tertiary-container rounded-full blur-[100px] translate-y-1/3 -translate-x-1/4"></div>
      </div>
      <div className="max-w-7xl mx-auto grid grid-cols-1 lg:grid-cols-2 gap-16 items-center relative z-10">
        <div className="space-y-8">
          <div className="inline-flex items-center px-4 py-2 rounded-full bg-white/10 backdrop-blur-md border border-white/20 text-on-primary-container text-sm font-semibold tracking-wide">
            <span className="material-symbols-outlined text-sm mr-2" style={{ fontVariationSettings: "'FILL' 1" }}>bolt</span>
            CÔNG NGHỆ CHẨN ĐOÁN THẾ HỆ MỚI
          </div>
          <h1 className="text-5xl md:text-7xl font-extrabold font-headline text-on-primary leading-[1.1] tracking-tight">
            Tương lai Y tế <br />
            <span className="text-secondary-fixed">Dẫn lối bởi AI.</span>
          </h1>
          <p className="text-xl text-on-primary/80 max-w-xl font-body leading-relaxed">
            Phân tích triệu chứng tức thì với độ chính xác lâm sàng cao. PharmaIntel kết nối dữ liệu y khoa khổng lồ để mang lại giải pháp chăm sóc sức khỏe cá nhân hóa.
          </p>
          <div className="flex flex-wrap gap-4">
            <button className="bg-gradient-to-r from-secondary-fixed to-secondary-fixed-dim text-on-secondary-fixed font-bold py-4 px-10 rounded-full text-lg shadow-xl shadow-primary/20 hover:scale-105 transition-transform">
              Chẩn đoán ngay
            </button>
            <button className="glass-panel text-on-surface font-semibold py-4 px-10 rounded-full border border-white/30 hover:bg-white transition-colors">
              Tìm hiểu thêm
            </button>
          </div>
        </div>
        <div className="relative group">
          <div className="absolute -inset-4 bg-gradient-to-tr from-secondary/30 to-primary/30 blur-2xl rounded-3xl group-hover:opacity-75 transition duration-1000"></div>
          <div className="relative glass-panel rounded-3xl p-6 border border-white/40 shadow-2xl">
            <img alt="AI Medical Interface" className="rounded-2xl w-full object-cover h-[400px]" data-alt="high-tech medical interface on a tablet screen showing complex 3d human anatomy with glowing blue data points and futuristic neural network overlays" src="https://lh3.googleusercontent.com/aida-public/AB6AXuBTh-ZB0L_LFsY-Mrb_EkIB3eyC2HSxtjSdHWxjuowzcvWmIt7TogzqstwBfNR7ksSx-lwzyY3edhOkaJ025T0XwjNlz4xfRwISbdUWrkrQb4Pgoc84-rfvEG4tQ4IhPlvqZUr4wJJXDwsS6mmf3ob1SYTTTViEGRzitmYAbj0JlnSyjww7y1JtWSBCrZdkhjGNKNwW09ovw36fdJbDLXggRU5Arp1CJAYiYF8M38kbJ38EKSH91IeH9UgKSx14O4OD3GxGciTt_jk" />
            <div className="absolute bottom-12 -left-8 bg-surface-container-lowest p-6 rounded-2xl shadow-xl border border-outline-variant/15 flex items-center gap-4">
              <div className="w-12 h-12 rounded-full bg-secondary-fixed flex items-center justify-center text-primary">
                <span className="material-symbols-outlined" style={{ fontVariationSettings: "'FILL' 1" }}>neurology</span>
              </div>
              <div>
                <div className="text-xs font-bold text-outline uppercase tracking-widest">Độ chính xác AI</div>
                <div className="text-2xl font-bold text-primary">99.2%</div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
