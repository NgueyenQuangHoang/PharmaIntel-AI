export function CurrentPrescriptions() {
  return (
    <section className="md:col-span-5 bg-surface-container-low p-8 rounded-xl shadow-sm border border-outline-variant/10">
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-xl font-bold">Đơn thuốc hiện tại</h2>
        <span className="material-symbols-outlined text-outline">medication</span>
      </div>
      
      <div className="space-y-6">
        <div className="flex gap-4 group hover:bg-surface-container-highest p-3 -mx-3 rounded-xl transition-colors cursor-pointer">
          <div className="w-12 h-12 bg-surface-container-lowest rounded-lg flex items-center justify-center shrink-0 shadow-sm border border-outline-variant/10">
            <span className="material-symbols-outlined text-primary">pill</span>
          </div>
          <div>
            <h3 className="font-bold group-hover:text-primary transition-colors">Đơn thuốc Viêm họng</h3>
            <p className="text-xs text-outline mb-2">Bác sĩ: Trần Thị B • 12/10/2024</p>
            <div className="flex flex-wrap gap-2">
              <span className="text-[10px] px-2 py-1 bg-surface-container-lowest rounded-md border border-outline-variant/30 font-medium text-on-surface-variant">Amoxicillin</span>
              <span className="text-[10px] px-2 py-1 bg-surface-container-lowest rounded-md border border-outline-variant/30 font-medium text-on-surface-variant">Ibuprofen</span>
            </div>
          </div>
        </div>
        
        <div className="flex gap-4 group hover:bg-surface-container-highest p-3 -mx-3 rounded-xl transition-colors cursor-pointer">
          <div className="w-12 h-12 bg-surface-container-lowest rounded-lg flex items-center justify-center shrink-0 shadow-sm border border-outline-variant/10">
            <span className="material-symbols-outlined text-secondary">vaccines</span>
          </div>
          <div>
            <h3 className="font-bold group-hover:text-primary transition-colors">Hỗ trợ Tim mạch</h3>
            <p className="text-xs text-outline mb-2">Bác sĩ: Lê Văn C • 05/09/2024</p>
            <div className="flex flex-wrap gap-2">
              <span className="text-[10px] px-2 py-1 bg-surface-container-lowest rounded-md border border-outline-variant/30 font-medium text-on-surface-variant">Omega 3</span>
              <span className="text-[10px] px-2 py-1 bg-surface-container-lowest rounded-md border border-outline-variant/30 font-medium text-on-surface-variant">Aspirin</span>
            </div>
          </div>
        </div>
      </div>
      
      <button className="mt-6 text-primary font-bold text-sm flex items-center gap-1 hover:text-primary-container transition-colors py-2">
        Xem tất cả đơn thuốc
        <span className="material-symbols-outlined text-sm">arrow_forward</span>
      </button>
    </section>
  );
}
