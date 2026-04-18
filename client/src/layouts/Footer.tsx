export function Footer() {
  return (
    <footer className="bg-slate-50 dark:bg-slate-950 w-full py-12 px-8 border-t border-slate-200 dark:border-slate-800">
      <div className="grid grid-cols-1 md:grid-cols-4 gap-8 max-w-7xl mx-auto">
        <div className="col-span-1 md:col-span-1">
          <div className="text-lg font-bold text-slate-900 dark:text-slate-100 font-headline mb-4">PharmaIntel AI</div>
          <p className="font-inter text-sm leading-relaxed text-slate-500 dark:text-slate-400">Đơn vị tiên phong trong ứng dụng trí tuệ nhân tạo vào lĩnh vực dược phẩm và chẩn đoán tại Việt Nam.</p>
        </div>
        <div>
          <div className="font-bold text-slate-900 dark:text-slate-100 mb-4">Dịch vụ</div>
          <ul className="space-y-2">
            <li><a className="font-inter text-sm text-slate-500 hover:text-slate-800 dark:text-slate-400 dark:hover:text-slate-200 transition-opacity hover:opacity-80" href="#">Chẩn đoán AI</a></li>
            <li><a className="font-inter text-sm text-slate-500 hover:text-slate-800 dark:text-slate-400 dark:hover:text-slate-200 transition-opacity hover:opacity-80" href="#">Đặt thuốc Online</a></li>
            <li><a className="font-inter text-sm text-slate-500 hover:text-slate-800 dark:text-slate-400 dark:hover:text-slate-200 transition-opacity hover:opacity-80" href="#">Tư vấn Bác sĩ</a></li>
          </ul>
        </div>
        <div>
          <div className="font-bold text-slate-900 dark:text-slate-100 mb-4">Thông tin</div>
          <ul className="space-y-2">
            <li><a className="font-inter text-sm text-slate-500 hover:text-slate-800 dark:text-slate-400 dark:hover:text-slate-200 transition-opacity hover:opacity-80" href="#">Điều khoản</a></li>
            <li><a className="font-inter text-sm text-slate-500 hover:text-slate-800 dark:text-slate-400 dark:hover:text-slate-200 transition-opacity hover:opacity-80" href="#">Bảo mật</a></li>
            <li><a className="font-inter text-sm text-slate-500 hover:text-slate-800 dark:text-slate-400 dark:hover:text-slate-200 transition-opacity hover:opacity-80" href="#">Miễn trừ trách nhiệm</a></li>
          </ul>
        </div>
        <div>
          <div className="font-bold text-slate-900 dark:text-slate-100 mb-4">Liên hệ</div>
          <p className="font-inter text-sm text-slate-500 dark:text-slate-400">Email: support@pharmaintel.ai</p>
          <p className="font-inter text-sm text-slate-500 dark:text-slate-400">Hotline: 1900 8888</p>
          <div className="flex gap-4 mt-4">
            <span className="material-symbols-outlined text-blue-700">public</span>
            <span className="material-symbols-outlined text-blue-700">medical_services</span>
          </div>
        </div>
      </div>
      <div className="max-w-7xl mx-auto mt-12 pt-8 border-t border-slate-200 dark:border-slate-800 text-center">
        <p className="font-inter text-sm text-slate-500">© 2024 PharmaIntel AI. Tất cả quyền được bảo lưu.</p>
      </div>
    </footer>
  );
}
