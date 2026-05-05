import { useAppDispatch, useAppSelector } from '@/hooks/redux';
import { addToCartThunk } from '@/features/cart/cart-slice';
import { calcFinalPrice, formatVnd } from '@/utils/format';
import type { DiagnosticSuggestedMedication } from '@/features/diagnostic/types';

const PLACEHOLDER_IMG =
  'https://lh3.googleusercontent.com/aida-public/AB6AXuBfZQ0ruQS33ro97Il1I21w9G1WjuJFgbv8KSVPHefZMNSmRGIYAVOVZ0Wvbodxy4H7VDECSPrs0eSPnlqxqP8Ex6vyXgD9jzdzvaKeCF5WVnSInhxdYI1n3L-8ITV0uNjWap14nU0dUXB5gYZu1akgyrDoZbJdaTJIOZKWcwhY7r7CVzK65iUXWzZvWpwJO6l_A8qh-CGbM_efHBQb2OESEaCeJbAfXmDDi42ccgDzosiHUsZaGJajFvsmAcRrbGFxdQZHXSazFz8';

function SuggestedCard({
  med,
  onAdd,
  isAdding,
  size = 'normal',
}: {
  med: DiagnosticSuggestedMedication;
  onAdd: () => void;
  isAdding: boolean;
  size?: 'normal' | 'large';
}) {
  const finalPrice = calcFinalPrice(med.price, med.discountPercent);
  if (size === 'large') {
    return (
      <article className="md:col-span-2 bg-surface-container-lowest rounded-xl overflow-hidden shadow-sm hover:shadow-md transition-shadow group border border-outline-variant/15 flex flex-col md:flex-row">
        <div className="md:w-1/3 aspect-square md:aspect-auto overflow-hidden bg-surface-container-low">
          <img
            alt={med.medicationName}
            className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
            src={med.imageUrl || PLACEHOLDER_IMG}
          />
        </div>
        <div className="p-8 flex-1 flex flex-col justify-between">
          <div>
            <div className="flex justify-between items-start mb-4">
              <div>
                <h3 className="font-headline text-2xl font-bold text-on-surface">
                  {med.medicationName}
                </h3>
                <span className="text-xs font-semibold text-on-tertiary-container px-2 py-1 rounded bg-tertiary-fixed uppercase tracking-wider inline-block mt-1">
                  Gợi ý chính
                </span>
              </div>
              <div className="text-right">
                <div className="text-2xl font-bold text-on-surface">{formatVnd(finalPrice)}</div>
                {med.discountPercent > 0 && (
                  <div className="text-sm text-on-surface-variant line-through">
                    {formatVnd(med.price)}
                  </div>
                )}
              </div>
            </div>
            {med.isPrescriptionRequired && (
              <p className="text-xs text-error mb-4">⚠ Cần đơn thuốc khi mua sản phẩm này.</p>
            )}
          </div>
          <button
            onClick={onAdd}
            disabled={isAdding}
            className="w-full py-4 bg-gradient-to-r from-primary to-primary-container text-on-primary font-bold rounded-full hover:opacity-90 transition-opacity flex items-center justify-center gap-2 disabled:opacity-50"
          >
            <span className="material-symbols-outlined" style={{ fontVariationSettings: "'FILL' 1" }}>
              {isAdding ? 'progress_activity' : 'shopping_cart'}
            </span>
            Mua ngay
          </button>
        </div>
      </article>
    );
  }

  return (
    <article className="bg-surface-container-lowest rounded-xl overflow-hidden shadow-sm hover:shadow-md transition-shadow group border border-outline-variant/15">
      <div className="aspect-video overflow-hidden bg-surface-container-low">
        <img
          alt={med.medicationName}
          className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
          src={med.imageUrl || PLACEHOLDER_IMG}
        />
      </div>
      <div className="p-6">
        <div className="flex justify-between items-start mb-4">
          <div>
            <h3 className="font-headline text-xl font-bold text-on-surface">
              {med.medicationName}
            </h3>
            <span className="text-xs font-semibold text-primary uppercase tracking-wider">
              Ưu tiên #{med.priority}
            </span>
          </div>
          <div className="text-right">
            <div className="text-lg font-bold text-on-surface">{formatVnd(finalPrice)}</div>
            {med.discountPercent > 0 && (
              <div className="text-xs text-on-surface-variant line-through">
                {formatVnd(med.price)}
              </div>
            )}
          </div>
        </div>
        {med.isPrescriptionRequired && (
          <p className="text-xs text-error mb-4">⚠ Cần đơn thuốc</p>
        )}
        <button
          onClick={onAdd}
          disabled={isAdding}
          className="w-full py-3 bg-gradient-to-r from-primary to-primary-container text-on-primary font-bold rounded-full hover:opacity-90 transition-opacity flex items-center justify-center gap-2 disabled:opacity-50"
        >
          <span className="material-symbols-outlined" style={{ fontVariationSettings: "'FILL' 1" }}>
            {isAdding ? 'progress_activity' : 'shopping_cart'}
          </span>
          Mua ngay
        </button>
      </div>
    </article>
  );
}

export function SuggestedMedications() {
  const dispatch = useAppDispatch();
  const session = useAppSelector((s) => s.diagnostic.currentSession);
  const pendingIds = useAppSelector((s) => s.cart.pendingMedicationIds);

  const meds = session?.result?.suggestedMedications ?? [];
  const sorted = meds.slice().sort((a, b) => a.priority - b.priority);

  const handleAdd = (medicationId: number) => {
    dispatch(addToCartThunk({ medicationId, quantity: 1 }));
  };

  return (
    <div className="space-y-8">
      <h2 className="text-2xl font-headline font-bold text-on-surface flex items-center">
        <span
          className="material-symbols-outlined mr-2 text-primary"
          style={{ fontVariationSettings: "'FILL' 1" }}
        >
          medication
        </span>
        Các loại thuốc được đề xuất
      </h2>

      {sorted.length === 0 && (
        <p className="text-on-surface-variant">
          AI chưa đề xuất thuốc cụ thể cho phiên này. Bạn nên tham khảo ý kiến bác sĩ trực tiếp.
        </p>
      )}

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {sorted.map((med, idx) => (
          <SuggestedCard
            key={med.id}
            med={med}
            isAdding={pendingIds.includes(med.medicationId)}
            onAdd={() => handleAdd(med.medicationId)}
            size={idx === 0 && sorted.length >= 3 ? 'large' : 'normal'}
          />
        ))}
      </div>
    </div>
  );
}
