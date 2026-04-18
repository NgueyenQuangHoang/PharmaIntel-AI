import { ResultHeader } from '@/features/diagnostic/components/ResultHeader';
import { SuggestedMedications } from '@/features/diagnostic/components/SuggestedMedications';
import { AISidebarInsight } from '@/features/diagnostic/components/AISidebarInsight';

export function DiagnosticResultPage() {
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
