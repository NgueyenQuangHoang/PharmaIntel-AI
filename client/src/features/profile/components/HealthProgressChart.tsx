export function HealthProgressChart() {
  const days = ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'];
  const data = [
    { primary: 'h-1/2', secondary: 'h-3/4', hoverPrimary: 'group-hover:h-3/5' },
    { primary: 'h-2/5', secondary: 'h-2/3', hoverPrimary: 'group-hover:h-1/2' },
    { primary: 'h-3/4', secondary: 'h-4/5', hoverPrimary: 'group-hover:h-4/5' },
    { primary: 'h-1/3', secondary: 'h-1/2', hoverPrimary: 'group-hover:h-2/5' },
    { primary: 'h-3/5', secondary: 'h-3/4', hoverPrimary: 'group-hover:h-4/5' },
    { primary: 'h-4/5', secondary: 'h-full', hoverPrimary: 'group-hover:h-5/6' },
    { primary: 'h-2/3', secondary: 'h-5/6', hoverPrimary: 'group-hover:h-3/4' },
  ];

  return (
    <section className="md:col-span-8 bg-surface-container-lowest p-8 rounded-xl shadow-sm border-l-4 border-primary">
      <div className="flex justify-between items-center mb-8">
        <h2 className="text-xl font-bold">Chỉ số sức khỏe tuần qua</h2>
        <div className="flex gap-2">
          <span className="px-3 py-1 bg-surface-container-low rounded-full text-xs font-medium">Huyết áp</span>
          <span className="px-3 py-1 bg-primary-fixed text-on-primary-fixed-variant rounded-full text-xs font-medium">Nhịp tim</span>
        </div>
      </div>
      
      <div className="h-64 flex items-end justify-between gap-4 px-2">
        {days.map((day, idx) => (
          <div key={day} className="flex-1 flex flex-col items-center gap-2">
            <div className="w-full bg-surface-container-low rounded-t-lg relative group h-40">
              <div className={`absolute bottom-0 w-full bg-primary/20 rounded-t-lg ${data[idx].secondary}`}></div>
              <div className={`absolute bottom-0 w-full bg-primary rounded-t-lg ${data[idx].primary} transition-all ${data[idx].hoverPrimary}`}></div>
            </div>
            <span className="text-xs text-outline">{day}</span>
          </div>
        ))}
      </div>
    </section>
  );
}
