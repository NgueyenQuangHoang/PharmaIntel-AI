export function MedicineSidebar() {
  return (
    <aside className="md:col-span-3 space-y-8">
      <div>
        <h3 className="text-xs font-bold tracking-widest text-outline uppercase mb-6">DANH MỤC SẢN PHẨM</h3>
        <ul className="space-y-2">
          <li>
            <button className="w-full flex items-center justify-between px-4 py-3 rounded-lg bg-primary-fixed text-on-primary-fixed-variant font-semibold transition-colors">
              <span>Tất cả sản phẩm</span>
              <span className="material-symbols-outlined text-sm">chevron_right</span>
            </button>
          </li>
          <li>
            <button className="w-full flex items-center justify-between px-4 py-3 rounded-lg text-on-surface-variant hover:bg-surface-container-high transition-colors">
              <span>GIẢM ĐAU</span>
              <span className="material-symbols-outlined text-sm">chevron_right</span>
            </button>
          </li>
          <li>
            <button className="w-full flex items-center justify-between px-4 py-3 rounded-lg text-on-surface-variant hover:bg-surface-container-high transition-colors">
              <span>KHÁNG SINH</span>
              <span className="material-symbols-outlined text-sm">chevron_right</span>
            </button>
          </li>
          <li>
            <button className="w-full flex items-center justify-between px-4 py-3 rounded-lg text-on-surface-variant hover:bg-surface-container-high transition-colors">
              <span>VITAMIN</span>
              <span className="material-symbols-outlined text-sm">chevron_right</span>
            </button>
          </li>
          <li>
            <button className="w-full flex items-center justify-between px-4 py-3 rounded-lg text-on-surface-variant hover:bg-surface-container-high transition-colors">
              <span>HỖ TRỢ TIÊU HÓA</span>
              <span className="material-symbols-outlined text-sm">chevron_right</span>
            </button>
          </li>
        </ul>
      </div>
      <div className="p-6 rounded-xl bg-gradient-to-br from-primary to-primary-container text-on-primary shadow-lg">
        <span className="material-symbols-outlined mb-4 text-secondary-fixed" style={{ fontVariationSettings: "'FILL' 1" }}>clinical_notes</span>
        <h4 className="font-headline font-bold mb-2">Tư vấn AI miễn phí</h4>
        <p className="text-sm text-on-primary/80 mb-4">Bạn không chắc nên mua loại nào? Hãy để AI giúp bạn phân tích triệu chứng.</p>
        <button className="w-full py-2 bg-surface-container-lowest text-primary hover:bg-white font-bold rounded-full text-sm transition-colors">Bắt đầu tư vấn</button>
      </div>
    </aside>
  );
}
