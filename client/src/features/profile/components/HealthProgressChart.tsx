import { useAppSelector } from '@/hooks/redux';

export function HealthProgressChart() {
  const { data, status } = useAppSelector((state) => state.profile.healthMetrics);

  // Generate last 7 days
  const days = Array.from({ length: 7 }).map((_, i) => {
    const d = new Date();
    d.setDate(d.getDate() - (6 - i));
    return d;
  });

  const dayLabels = days.map((d) => {
    const dayNames = ['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7'];
    return dayNames[d.getDay()];
  });

  const calculateHeights = (dayDate: Date) => {
    if (!data) return { primary: 'h-0', secondary: 'h-0', hoverPrimary: 'group-hover:h-0' };
    
    // Find metric for this day
    const metricStrDate = dayDate.toISOString().split('T')[0];
    const dayMetrics = data.items.filter(m => m.recordedAt.startsWith(metricStrDate));
    
    if (dayMetrics.length === 0) {
      // Mock fallback if no real data
      return { primary: 'h-0', secondary: 'h-0', hoverPrimary: 'group-hover:h-0' };
    }

    // Try to find blood pressure and heart rate
    const bp = dayMetrics.find(m => m.metricType === 'blood_pressure');
    const hr = dayMetrics.find(m => m.metricType === 'heart_rate');
    
    // Fake logic to map to tailwind heights based on values (120 => 60%, 80 => 40%)
    let hPrimary = 0;
    let hSecondary = 0;
    
    if (hr) {
      hPrimary = Math.min(100, Math.max(10, Math.round((hr.valueNumber / 120) * 100)));
    }
    if (bp) {
      hSecondary = Math.min(100, Math.max(10, Math.round((bp.valueNumber / 180) * 100)));
    }
    
    return {
      primary: `h-[${hPrimary}%]`,
      secondary: `h-[${hSecondary}%]`,
      hoverPrimary: `group-hover:h-[${Math.min(100, hPrimary + 10)}%]`
    };
  };

  return (
    <section className="md:col-span-8 bg-surface-container-lowest p-8 rounded-xl shadow-sm border-l-4 border-primary">
      <div className="flex justify-between items-center mb-8">
        <h2 className="text-xl font-bold">Chỉ số sức khỏe tuần qua</h2>
        <div className="flex gap-2 items-center">
          {status === 'loading' && <span className="text-xs text-primary animate-pulse mr-2">Đang tải...</span>}
          <span className="px-3 py-1 bg-surface-container-low rounded-full text-xs font-medium">Huyết áp</span>
          <span className="px-3 py-1 bg-primary-fixed text-on-primary-fixed-variant rounded-full text-xs font-medium">Nhịp tim</span>
        </div>
      </div>
      
      <div className="h-64 flex items-end justify-between gap-4 px-2">
        {days.map((day, idx) => {
          const heights = calculateHeights(day);
          // If using arbitrary tailwind heights like h-[60%], they need to be safelisted or injected via style
          // For simplicity in dynamic heights, let's use style
          const bpMetric = data?.items.find(m => m.recordedAt.startsWith(day.toISOString().split('T')[0]) && m.metricType === 'blood_pressure');
          const hrMetric = data?.items.find(m => m.recordedAt.startsWith(day.toISOString().split('T')[0]) && m.metricType === 'heart_rate');
          
          let hPrimary = hrMetric ? Math.min(100, Math.max(10, Math.round((hrMetric.valueNumber / 120) * 100))) : 0;
          let hSecondary = bpMetric ? Math.min(100, Math.max(10, Math.round((bpMetric.valueNumber / 180) * 100))) : 0;
          
          // Use mock values if no data for the day just to show the chart visual since we're in dev
          if (!data || data.items.length === 0) {
             const mockData = [
               { p: 50, s: 75 }, { p: 40, s: 66 }, { p: 75, s: 80 }, 
               { p: 33, s: 50 }, { p: 60, s: 75 }, { p: 80, s: 100 }, { p: 66, s: 83 }
             ];
             hPrimary = mockData[idx].p;
             hSecondary = mockData[idx].s;
          }

          return (
            <div key={idx} className="flex-1 flex flex-col items-center gap-2">
              <div className="w-full bg-surface-container-low rounded-t-lg relative group h-40">
                <div 
                  className="absolute bottom-0 w-full bg-primary/20 rounded-t-lg transition-all"
                  style={{ height: `${hSecondary}%` }}
                  title={bpMetric ? `Huyết áp: ${bpMetric.valueNumber}/${bpMetric.valueNumber2 || ''}` : 'Không có dữ liệu huyết áp'}
                ></div>
                <div 
                  className="absolute bottom-0 w-full bg-primary rounded-t-lg transition-all hover:brightness-110"
                  style={{ height: `${hPrimary}%` }}
                  title={hrMetric ? `Nhịp tim: ${hrMetric.valueNumber} bpm` : 'Không có dữ liệu nhịp tim'}
                ></div>
              </div>
              <span className="text-xs text-outline">{dayLabels[idx]}</span>
            </div>
          );
        })}
      </div>
    </section>
  );
}
