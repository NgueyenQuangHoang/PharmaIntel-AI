export function DiagnosisHistory() {
  return (
    <section className="md:col-span-7 glass-panel p-8 rounded-xl ambient-shadow">
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-xl font-bold">Lịch sử chẩn đoán AI</h2>
        <span className="text-xs text-primary bg-primary-fixed px-3 py-1 rounded-full font-semibold">Cập nhật 2 giờ trước</span>
      </div>
      
      <div className="overflow-x-auto">
        <table className="w-full text-left">
          <thead>
            <tr className="border-b border-outline-variant/20">
              <th className="pb-4 text-xs font-bold text-outline uppercase tracking-wider">Ngày</th>
              <th className="pb-4 text-xs font-bold text-outline uppercase tracking-wider">Triệu chứng</th>
              <th className="pb-4 text-xs font-bold text-outline uppercase tracking-wider">Kết luận AI</th>
              <th className="pb-4 text-xs font-bold text-outline uppercase tracking-wider">Độ tin cậy</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-outline-variant/10">
            <tr className="group hover:bg-surface-container-low/50 transition-colors cursor-pointer">
              <td className="py-4 text-sm font-medium">20/10/2024</td>
              <td className="py-4 text-sm">Đau đầu, sốt nhẹ</td>
              <td className="py-4 text-sm font-bold text-primary">Cúm mùa</td>
              <td className="py-4">
                <div className="w-24 bg-surface-container-low h-1.5 rounded-full overflow-hidden">
                  <div className="bg-primary h-full rounded-full w-[92%]"></div>
                </div>
              </td>
            </tr>
            <tr className="group hover:bg-surface-container-low/50 transition-colors cursor-pointer">
              <td className="py-4 text-sm font-medium">15/09/2024</td>
              <td className="py-4 text-sm">Đau tức ngực</td>
              <td className="py-4 text-sm font-bold text-secondary">Trào ngược dạ dày</td>
              <td className="py-4">
                <div className="w-24 bg-surface-container-low h-1.5 rounded-full overflow-hidden">
                  <div className="bg-secondary h-full rounded-full w-[85%]"></div>
                </div>
              </td>
            </tr>
            <tr className="group hover:bg-surface-container-low/50 transition-colors cursor-pointer">
              <td className="py-4 text-sm font-medium">02/08/2024</td>
              <td className="py-4 text-sm">Phát ban cánh tay</td>
              <td className="py-4 text-sm font-bold text-primary">Viêm da cơ địa</td>
              <td className="py-4">
                <div className="w-24 bg-surface-container-low h-1.5 rounded-full overflow-hidden">
                  <div className="bg-primary h-full rounded-full w-[78%]"></div>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>
  );
}
