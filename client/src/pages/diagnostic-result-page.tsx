import { useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { ResultHeader } from '@/features/diagnostic/components/ResultHeader';
import { SuggestedMedications } from '@/features/diagnostic/components/SuggestedMedications';
import { AISidebarInsight } from '@/features/diagnostic/components/AISidebarInsight';
import { useAppDispatch, useAppSelector } from '@/hooks/redux';
import { fetchSessionThunk } from '@/features/diagnostic/diagnostic-slice';

export function DiagnosticResultPage() {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const [searchParams] = useSearchParams();
  const sessionIdParam = searchParams.get('sessionId');

  const session = useAppSelector((s) => s.diagnostic.currentSession);
  const sessionStatus = useAppSelector((s) => s.diagnostic.sessionStatus);

  useEffect(() => {
    if (!sessionIdParam) {
      navigate('/diagnostic', { replace: true });
      return;
    }
    const id = Number(sessionIdParam);
    if (!session || session.id !== id) {
      dispatch(fetchSessionThunk(id));
    }
  }, [sessionIdParam, session, dispatch, navigate]);

  if (sessionStatus === 'loading' && !session) {
    return (
      <div className="pt-8 pb-16 px-4 md:px-8 max-w-7xl mx-auto">
        <p className="text-on-surface-variant">Đang tải kết quả...</p>
      </div>
    );
  }

  if (!session) {
    return (
      <div className="pt-8 pb-16 px-4 md:px-8 max-w-7xl mx-auto">
        <p className="text-on-surface-variant">Không tìm thấy phiên chẩn đoán.</p>
      </div>
    );
  }

  return (
    <div className="pt-8 pb-16 px-4 md:px-8 max-w-7xl mx-auto animate-in fade-in zoom-in-95 duration-500">
      <ResultHeader />
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-8 items-start">
        <div className="lg:col-span-8">
          <SuggestedMedications />
        </div>
        <div className="lg:col-span-4">
          <AISidebarInsight />
        </div>
      </div>
    </div>
  );
}
