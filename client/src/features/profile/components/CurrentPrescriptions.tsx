import { useAppSelector } from '@/hooks/redux';

export function CurrentPrescriptions() {
  const { data, status } = useAppSelector((state) => state.profile.prescriptions);

  return (
    <section className="md:col-span-5 bg-surface-container-low p-8 rounded-xl shadow-sm border border-outline-variant/10">
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-xl font-bold">Đơn thuốc hiện tại</h2>
        <span className="material-symbols-outlined text-outline">medication</span>
      </div>
      
      <div className="space-y-6">
        {status === 'loading' && !data && (
          <div className="text-center text-outline py-4 animate-pulse">Đang tải...</div>
        )}

        {status === 'success' && (!data || data.items.length === 0) && (
          <div className="text-center text-outline py-4">Chưa có đơn thuốc nào</div>
        )}

        {data?.items.map((prescription) => (
          <div key={prescription.id} className="flex gap-4 group hover:bg-surface-container-highest p-3 -mx-3 rounded-xl transition-colors cursor-pointer">
            <div className="w-12 h-12 bg-surface-container-lowest rounded-lg flex items-center justify-center shrink-0 shadow-sm border border-outline-variant/10">
              <span className="material-symbols-outlined text-primary">pill</span>
            </div>
            <div>
              <h3 className="font-bold group-hover:text-primary transition-colors">
                {prescription.title || `Đơn thuốc #${prescription.id}`}
              </h3>
              <p className="text-xs text-outline mb-2">
                Bác sĩ: {prescription.doctorNameSnapshot || 'Không rõ'} • {new Date(prescription.createdAt).toLocaleDateString('vi-VN')}
              </p>
              <div className="flex flex-wrap gap-2">
                {prescription.itemCount > 0 ? (
                  <span className="text-[10px] px-2 py-1 bg-surface-container-lowest rounded-md border border-outline-variant/30 font-medium text-on-surface-variant">
                    Gồm {prescription.itemCount} loại thuốc
                  </span>
                ) : (
                  <span className="text-[10px] px-2 py-1 bg-surface-container-lowest rounded-md border border-outline-variant/30 font-medium text-on-surface-variant text-error">
                    Trống
                  </span>
                )}
                {prescription.status === 'completed' && (
                  <span className="text-[10px] px-2 py-1 bg-primary/10 text-primary rounded-md border border-primary/20 font-medium">
                    Đã hoàn thành
                  </span>
                )}
              </div>
            </div>
          </div>
        ))}
      </div>
      
      {data && data.totalCount > data.items.length && (
        <button className="mt-6 text-primary font-bold text-sm flex items-center gap-1 hover:text-primary-container transition-colors py-2">
          Xem tất cả đơn thuốc
          <span className="material-symbols-outlined text-sm">arrow_forward</span>
        </button>
      )}
    </section>
  );
}
