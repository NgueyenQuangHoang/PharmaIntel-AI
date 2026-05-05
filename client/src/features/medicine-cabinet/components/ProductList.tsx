import { useEffect } from 'react';
import { useAppDispatch, useAppSelector } from '@/hooks/redux';
import { fetchCatalogThunk } from '@/features/medications/medications-slice';
import { addToCartThunk } from '@/features/cart/cart-slice';
import { formatVnd } from '@/utils/format';
import type { MedicationListItem } from '@/features/medications/types';

interface ProductListProps {
  selectedCategoryId: number | null;
  onOpenCart?: () => void;
}

const PLACEHOLDER_IMG = 'https://lh3.googleusercontent.com/aida-public/AB6AXuCwiMzRPJTukQFx_9X03mgt9E1xTAVTeUFI7p6ZozhZF5QLvtU9hg5J02YUiuOFb6vsvHAwFBrnetN8IyNa8ONYnLpkeh5hyOchV--12uJhWJUVNwCy4CBYa7UpHVMZwy0kaS_NBLJnrLkyWJELLD8lV4e6B4VShEKUoSyR6mRl8Kq-lflELOU9TvyeIeg1uftP_OUeu6glcOVnuHoQV2Pxr5FnjgQ-bgj7k2hlLML9J2YBvTsWVx7s_1sln4J0zNng3S_XlLnvJtk';

function ProductCard({
  med,
  onAdd,
  isAdding,
}: {
  med: MedicationListItem;
  onAdd: () => void;
  isAdding: boolean;
}) {
  return (
    <div className="glass-card rounded-xl overflow-hidden flex flex-col group transition-all hover:-translate-y-1">
      <div className="aspect-square relative bg-surface-container-low">
        <img
          alt={med.name}
          className="w-full h-full object-contain p-8"
          src={med.imageUrl || PLACEHOLDER_IMG}
        />
        <span className="absolute top-4 left-4 bg-secondary-container text-on-secondary-container text-xs font-bold px-3 py-1 rounded-full uppercase tracking-wider">
          {med.categoryName}
        </span>
        {med.discountPercent > 0 && (
          <span className="absolute top-4 right-4 bg-error-container text-on-error-container text-[10px] font-bold px-2 py-1 rounded-lg uppercase">
            -{med.discountPercent}%
          </span>
        )}
      </div>
      <div className="p-6 flex flex-col flex-1">
        <h3 className="font-headline font-bold text-lg mb-1 group-hover:text-primary transition-colors">
          {med.name}
        </h3>
        <p className="text-sm text-on-surface-variant mb-4 line-clamp-2">
          {med.manufacturer ?? med.genericName ?? ''}
        </p>
        <div className="mt-auto flex items-center justify-between">
          <div className="flex flex-col">
            <span className="text-xl font-bold text-primary">{formatVnd(med.finalPrice)}</span>
            {med.discountPercent > 0 && (
              <span className="text-xs text-on-surface-variant line-through">
                {formatVnd(med.price)}
              </span>
            )}
          </div>
          <button
            onClick={onAdd}
            disabled={isAdding || med.stockQuantity <= 0}
            className="w-10 h-10 rounded-full bg-primary text-on-primary flex items-center justify-center active:scale-90 transition-transform shadow-md hover:opacity-90 disabled:opacity-50 disabled:cursor-not-allowed"
            title={med.stockQuantity <= 0 ? 'Hết hàng' : 'Thêm vào giỏ'}
          >
            <span
              className="material-symbols-outlined"
              style={{ fontVariationSettings: "'FILL' 1" }}
            >
              {isAdding ? 'progress_activity' : 'add_shopping_cart'}
            </span>
          </button>
        </div>
      </div>
    </div>
  );
}

export function ProductList({ selectedCategoryId, onOpenCart }: ProductListProps) {
  const dispatch = useAppDispatch();
  const { items, status, error } = useAppSelector((s) => s.medications.catalog);
  const pendingIds = useAppSelector((s) => s.cart.pendingMedicationIds);

  useEffect(() => {
    dispatch(
      fetchCatalogThunk({
        categoryId: selectedCategoryId ?? undefined,
      }),
    );
  }, [selectedCategoryId, dispatch]);

  const handleAdd = async (medicationId: number) => {
    try {
      await dispatch(addToCartThunk({ medicationId, quantity: 1 })).unwrap();
      onOpenCart?.();
    } catch {
      /* error in slice */
    }
  };

  return (
    <section className="md:col-span-9">
      {status === 'loading' && items.length === 0 && (
        <div className="text-center text-on-surface-variant py-20">Đang tải sản phẩm...</div>
      )}

      {status === 'error' && (
        <div className="rounded-lg bg-error-container/40 border border-error/30 px-4 py-3 text-sm text-error">
          {error ?? 'Không tải được danh sách thuốc'}
        </div>
      )}

      {status !== 'loading' && items.length === 0 && (
        <div className="text-center text-on-surface-variant py-20">
          Không có sản phẩm nào trong danh mục này.
        </div>
      )}

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
        {items.map((med) => (
          <ProductCard
            key={med.id}
            med={med}
            onAdd={() => handleAdd(med.id)}
            isAdding={pendingIds.includes(med.id)}
          />
        ))}
      </div>
    </section>
  );
}
