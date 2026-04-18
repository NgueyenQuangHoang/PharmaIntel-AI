import { DiagnosticHeader } from '@/features/diagnostic/components/DiagnosticHeader';
import { DiagnosticChat } from '@/features/diagnostic/components/DiagnosticChat';
import { DiagnosticSidebar } from '@/features/diagnostic/components/DiagnosticSidebar';

export function DiagnosticPage() {
  return (
    <div className="pt-8 pb-16 px-4 md:px-8 max-w-7xl mx-auto">
      <DiagnosticHeader />
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-8 items-start">
        <div className="lg:col-span-7">
          <DiagnosticChat />
        </div>
        <div className="lg:col-span-5">
          <DiagnosticSidebar />
        </div>
      </div>
    </div>
  );
}
