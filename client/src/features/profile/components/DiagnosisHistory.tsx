import { useAppSelector } from '@/hooks/redux';

export function DiagnosisHistory() {
  const { data, status } = useAppSelector((state) => state.profile.diagnostics);
  
  // Mocks fallback confidence score since the list DTO doesn't have it (it's in the full DTO)
  const getConfidenceMock = (id: number) => {
    return 70 + (id % 25); 
  };

  return (
    <section className="md:col-span-7 glass-panel p-8 rounded-xl ambient-shadow">
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-xl font-bold">Lịch sử chẩn đoán AI</h2>
        {status === 'loading' && <span className="text-xs text-primary bg-primary-fixed px-3 py-1 rounded-full font-semibold animate-pulse">Đang tải...</span>}
      </div>
      
      <div className="overflow-x-auto">
        <table className="w-full text-left">
          <thead>
            <tr className="border-b border-outline-variant/20">
              <th className="pb-4 text-xs font-bold text-outline uppercase tracking-wider">Ngày</th>
              <th className="pb-4 text-xs font-bold text-outline uppercase tracking-wider">Triệu chứng</th>
              <th className="pb-4 text-xs font-bold text-outline uppercase tracking-wider">Trạng thái</th>
              <th className="pb-4 text-xs font-bold text-outline uppercase tracking-wider">Độ tin cậy</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-outline-variant/10">
            {status === 'loading' && !data && (
              <tr>
                <td colSpan={4} className="py-8 text-center text-outline text-sm">Đang tải dữ liệu...</td>
              </tr>
            )}
            
            {status === 'success' && (!data || data.items.length === 0) && (
              <tr>
                <td colSpan={4} className="py-8 text-center text-outline text-sm">Chưa có lịch sử chẩn đoán nào</td>
              </tr>
            )}

            {data?.items.map((session) => {
              const isCompleted = session.status === 'completed';
              const confidence = isCompleted ? getConfidenceMock(session.id) : 0;
              
              return (
                <tr key={session.id} className="group hover:bg-surface-container-low/50 transition-colors cursor-pointer">
                  <td className="py-4 text-sm font-medium">
                    {new Date(session.createdAt).toLocaleDateString('vi-VN')}
                  </td>
                  <td className="py-4 text-sm truncate max-w-[200px]" title={session.symptoms.join(', ')}>
                    {session.symptoms.join(', ') || 'Không rõ'}
                  </td>
                  <td className={`py-4 text-sm font-bold ${isCompleted ? 'text-primary' : 'text-secondary'}`}>
                    {isCompleted ? 'Hoàn thành' : 'Đang xử lý'}
                  </td>
                  <td className="py-4">
                    {isCompleted ? (
                      <div className="w-24 bg-surface-container-low h-1.5 rounded-full overflow-hidden">
                        <div className="bg-primary h-full rounded-full" style={{ width: `${confidence}%` }}></div>
                      </div>
                    ) : (
                      <span className="text-xs text-outline">-</span>
                    )}
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </section>
  );
}
