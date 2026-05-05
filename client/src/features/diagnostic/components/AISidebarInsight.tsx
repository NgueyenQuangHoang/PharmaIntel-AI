import { useAppSelector } from '@/hooks/redux';
import type { DiagnosticRiskLevel } from '@/features/diagnostic/types';

const RISK_LABEL: Record<DiagnosticRiskLevel, string> = {
  low: 'Thấp',
  medium: 'Trung bình',
  high: 'Cao',
  emergency: 'Khẩn cấp',
};

const RISK_TONE: Record<DiagnosticRiskLevel, string> = {
  low: 'bg-secondary-fixed text-on-secondary-fixed',
  medium: 'bg-tertiary-fixed text-on-tertiary-fixed',
  high: 'bg-error-container text-on-error-container',
  emergency: 'bg-error text-on-error',
};

export function AISidebarInsight() {
  const session = useAppSelector((s) => s.diagnostic.currentSession);
  const result = session?.result ?? null;
  const risk = result?.riskLevel ?? 'low';
  const confidence = result ? Math.round(result.confidenceScore) : null;

  return (
    <aside className="space-y-8">
      <section className="bg-surface-container-lowest p-8 rounded-xl border border-outline-variant/15 border-l-4 border-l-primary shadow-sm">
        <h3 className="font-headline text-xl font-bold text-on-surface mb-4 flex items-center">
          <span
            className="material-symbols-outlined mr-2 text-primary"
            style={{ fontVariationSettings: "'FILL' 1" }}
          >
            lightbulb
          </span>
          Lời khuyên từ AI
        </h3>

        {result ? (
          <div className="space-y-4">
            <div className="flex items-center gap-3">
              <span
                className={`text-xs font-bold px-3 py-1 rounded-full uppercase tracking-wider ${RISK_TONE[risk]}`}
              >
                Mức độ: {RISK_LABEL[risk]}
              </span>
              {confidence !== null && (
                <span className="text-xs text-on-surface-variant">Độ tin cậy {confidence}%</span>
              )}
            </div>

            {result.aiConclusion && (
              <p className="text-on-surface-variant leading-relaxed whitespace-pre-wrap">
                {result.aiConclusion}
              </p>
            )}

            {result.redFlags && (
              <div className="rounded-lg bg-error-container/30 border border-error/20 p-3">
                <p className="text-xs font-bold text-error mb-1 uppercase tracking-wider">
                  Dấu hiệu cảnh báo
                </p>
                <p className="text-sm text-on-surface whitespace-pre-wrap">{result.redFlags}</p>
              </div>
            )}

            {result.requiresDoctorVisit && (
              <p className="text-sm font-semibold text-error">
                ⚠ AI khuyến nghị bạn nên gặp bác sĩ trực tiếp.
              </p>
            )}

            {(result.modelName || result.modelVersion) && (
              <p className="text-[10px] text-outline">
                Model: {result.modelName ?? '?'} {result.modelVersion ?? ''}
              </p>
            )}
          </div>
        ) : (
          <p className="text-on-surface-variant text-sm">
            Chưa có phân tích AI cho phiên này.
          </p>
        )}
      </section>

      <section className="bg-error-container/20 p-8 rounded-xl border border-error/10">
        <div className="flex items-center text-error font-bold mb-4">
          <span className="material-symbols-outlined mr-2">warning</span>
          Miễn trừ trách nhiệm y tế
        </div>
        <div className="text-xs text-on-surface-variant space-y-3 leading-relaxed">
          <p>
            Thông tin được cung cấp bởi <span className="font-bold">PharmaIntel AI</span> chỉ mang
            tính chất tham khảo và không thay thế cho lời khuyên, chẩn đoán hoặc điều trị chuyên
            môn từ bác sĩ y khoa.
          </p>
          <p>
            Luôn tìm kiếm lời khuyên từ bác sĩ của bạn hoặc nhà cung cấp dịch vụ y tế có trình độ
            chuyên môn khác nếu bạn có bất kỳ câu hỏi nào liên quan đến tình trạng sức khỏe.
          </p>
          <p className="font-semibold text-on-surface">
            Trong trường hợp khẩn cấp, hãy liên hệ ngay với trung tâm cấp cứu gần nhất (115).
          </p>
        </div>
      </section>

      <section className="bg-surface-container-high p-8 rounded-xl text-center">
        <div className="w-16 h-16 bg-white rounded-full flex items-center justify-center mx-auto mb-4 shadow-sm">
          <span className="material-symbols-outlined text-primary text-3xl">support_agent</span>
        </div>
        <h4 className="font-headline font-bold text-on-surface mb-2">Cần tư vấn trực tiếp?</h4>
        <p className="text-sm text-on-surface-variant mb-6">
          Kết nối ngay với dược sĩ chuyên môn để được tư vấn chi tiết hơn.
        </p>
        <button className="w-full py-2 px-6 rounded-full border-2 border-primary text-primary font-bold hover:bg-primary hover:text-on-primary transition-all">
          Trò chuyện với Dược sĩ
        </button>
      </section>
    </aside>
  );
}
