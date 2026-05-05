import { useEffect, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '@/hooks/redux';
import {
  completeSessionThunk,
  createSessionThunk,
  fetchSymptomsThunk,
  resetSession,
  toggleSymptom,
} from '@/features/diagnostic/diagnostic-slice';
import type { Symptom } from '@/features/diagnostic/types';

export function DiagnosticSidebar() {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();

  const {
    symptoms,
    symptomsStatus,
    selectedSymptomIds,
    completing,
    sessionStatus,
    error,
  } = useAppSelector((s) => s.diagnostic);

  const isAnalyzing = completing || sessionStatus === 'loading';

  useEffect(() => {
    if (symptomsStatus === 'idle') {
      dispatch(fetchSymptomsThunk());
    }
  }, [symptomsStatus, dispatch]);

  // Group symptoms theo groupName
  const grouped = useMemo(() => {
    const map = new Map<string, Symptom[]>();
    for (const s of symptoms) {
      const key = s.groupName ?? 'Khác';
      if (!map.has(key)) map.set(key, []);
      map.get(key)!.push(s);
    }
    return Array.from(map.entries());
  }, [symptoms]);

  const handleAnalyze = async () => {
    if (selectedSymptomIds.length === 0 || isAnalyzing) return;
    try {
      // Reset phien cu (neu co) truoc khi tao moi
      dispatch(resetSession());
      const session = await dispatch(
        createSessionThunk({ symptomIds: selectedSymptomIds }),
      ).unwrap();
      const completed = await dispatch(completeSessionThunk(session.id)).unwrap();
      navigate(`/diagnostic/result?sessionId=${completed.id}`);
    } catch {
      /* error in slice */
    }
  };

  return (
    <div className="space-y-6 relative">
      {isAnalyzing && (
        <div className="absolute inset-0 z-50 bg-surface/60 backdrop-blur-sm rounded-xl flex flex-col items-center justify-center p-8 border border-primary/20">
          <div className="w-12 h-12 border-4 border-primary/30 border-t-primary rounded-full animate-spin mb-4"></div>
          <h3 className="text-xl font-bold font-headline text-primary mb-2">
            Đang phân tích dữ liệu...
          </h3>
          <p className="text-sm text-on-surface-variant text-center">
            AI đang tổng hợp các triệu chứng và tìm kiếm phác đồ phù hợp nhất với bạn.
          </p>
        </div>
      )}

      <section className="bg-surface-container-lowest rounded-xl p-8 shadow-sm border border-outline-variant/15">
        <h2 className="text-xl font-bold text-on-surface mb-6 flex items-center gap-2">
          <span className="material-symbols-outlined text-secondary">checklist</span>
          Danh mục triệu chứng
        </h2>

        {symptomsStatus === 'loading' && symptoms.length === 0 && (
          <p className="text-sm text-on-surface-variant">Đang tải triệu chứng...</p>
        )}
        {symptomsStatus === 'error' && (
          <p className="text-sm text-error">Không tải được triệu chứng</p>
        )}

        <div className="space-y-4">
          {grouped.map(([group, list]) => (
            <div key={group} className="group">
              <p className="text-xs font-bold text-outline uppercase tracking-wider mb-3">
                {group}
              </p>
              <div className="flex flex-wrap gap-2">
                {list.map((sym) => {
                  const active = selectedSymptomIds.includes(sym.id);
                  return (
                    <button
                      key={sym.id}
                      onClick={() => dispatch(toggleSymptom(sym.id))}
                      className={`px-4 py-2 rounded-full text-sm font-medium transition-all flex items-center gap-2 ${
                        active
                          ? 'border border-primary text-primary bg-primary-fixed'
                          : 'border border-outline-variant hover:border-primary hover:text-primary bg-surface'
                      }`}
                    >
                      <span>{sym.name}</span>
                      <span className="material-symbols-outlined text-xs">
                        {active ? 'check' : 'add'}
                      </span>
                    </button>
                  );
                })}
              </div>
            </div>
          ))}
        </div>

        {error && (
          <div className="mt-4 rounded-lg bg-error-container/40 border border-error/30 px-3 py-2 text-xs text-error">
            {error}
          </div>
        )}

        <div className="mt-10 pt-8 border-t border-outline-variant/15">
          <button
            onClick={handleAnalyze}
            disabled={isAnalyzing || selectedSymptomIds.length === 0}
            className="w-full py-4 rounded-full bg-gradient-to-r from-primary to-primary-container text-on-primary font-bold text-lg shadow-lg shadow-primary/20 hover:scale-[1.02] active:scale-[0.98] transition-transform flex items-center justify-center gap-3 disabled:opacity-60 disabled:hover:scale-100 disabled:cursor-not-allowed"
          >
            <span
              className="material-symbols-outlined"
              style={{ fontVariationSettings: "'FILL' 1" }}
            >
              analytics
            </span>
            Phân tích &amp; Kê thuốc
            {selectedSymptomIds.length > 0 && (
              <span className="text-sm opacity-80">({selectedSymptomIds.length})</span>
            )}
          </button>
          {selectedSymptomIds.length === 0 && (
            <p className="text-xs text-on-surface-variant text-center mt-3">
              Chọn ít nhất 1 triệu chứng để bắt đầu.
            </p>
          )}
        </div>
      </section>

      <section className="bg-surface-container-low rounded-xl p-8 border-l-4 border-l-secondary">
        <h3 className="text-lg font-bold text-on-surface flex items-center gap-2 mb-3">
          <span className="material-symbols-outlined text-secondary">info</span>
          Hướng dẫn chẩn đoán
        </h3>
        <ul className="space-y-3 text-on-surface-variant text-sm leading-relaxed">
          <li className="flex gap-2">
            <span className="text-secondary font-bold">•</span>
            Mô tả chi tiết thời gian bắt đầu và mức độ đau.
          </li>
          <li className="flex gap-2">
            <span className="text-secondary font-bold">•</span>
            Ghi chú các yếu tố làm tăng hoặc giảm triệu chứng.
          </li>
          <li className="flex gap-2">
            <span className="text-secondary font-bold">•</span>
            Nếu có các dấu hiệu nguy cấp như khó thở, hãy gọi cấp cứu ngay lập tức.
          </li>
        </ul>
      </section>
    </div>
  );
}
