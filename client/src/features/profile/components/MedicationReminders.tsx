export function MedicationReminders() {
  return (
    <aside className="md:col-span-4 bg-primary-fixed p-8 rounded-xl flex flex-col">
      <div className="flex items-center gap-3 mb-6">
        <span className="material-symbols-outlined text-primary">alarm</span>
        <h2 className="text-xl font-bold text-on-primary-fixed">Nhắc nhở uống thuốc</h2>
      </div>
      <div className="space-y-4 flex-grow">
        <div className="bg-surface-container-lowest p-4 rounded-lg flex items-center justify-between shadow-sm">
          <div>
            <p className="font-bold text-primary">Amoxicillin</p>
            <p className="text-xs text-outline">500mg • Sau ăn</p>
          </div>
          <div className="text-right">
            <p className="text-sm font-bold">08:00</p>
            <span className="text-[10px] bg-secondary-container px-2 py-0.5 rounded text-on-secondary-container font-medium">Sắp tới</span>
          </div>
        </div>
        
        <div className="bg-surface-container-lowest p-4 rounded-lg flex items-center justify-between opacity-60">
          <div>
            <p className="font-bold text-on-surface">Paracetamol</p>
            <p className="text-xs text-outline">500mg • Khi đau</p>
          </div>
          <div className="text-right">
            <p className="text-sm font-bold text-on-surface-variant line-through">12:30</p>
            <span className="material-symbols-outlined text-green-600">check_circle</span>
          </div>
        </div>
        
        <div className="bg-surface-container-lowest p-4 rounded-lg flex items-center justify-between shadow-sm">
          <div>
            <p className="font-bold text-primary">Vitamin C</p>
            <p className="text-xs text-outline">1000mg • Sáng</p>
          </div>
          <div className="text-right">
            <p className="text-sm font-bold">20:00</p>
          </div>
        </div>
      </div>
      <button className="mt-6 w-full py-3 bg-surface-container-lowest text-primary text-sm font-bold rounded-full border border-primary/10 hover:bg-surface-container-low transition-colors shadow-sm">
        Quản lý lịch nhắc
      </button>
    </aside>
  );
}
