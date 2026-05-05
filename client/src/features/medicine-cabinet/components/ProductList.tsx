import { useEffect } from 'react';
import { useAppDispatch, useAppSelector } from '@/hooks/redux';
import { fetchCatalogThunk } from '@/features/medications/medications-slice';
import { addToCartThunk } from '@/features/cart/cart-slice';
import { formatVnd } from '@/utils/format';
import type { MedicationListItem } from '@/features/medications/types';

interface ProductListProps {
  selectedCategoryId: number | null;
  searchQuery?: string;
  sortOption?: string;
  page: number;
  onPageChange: (page: number) => void;
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

export function ProductList({ selectedCategoryId, searchQuery, sortOption, page, onPageChange, onOpenCart }: ProductListProps) {
  const dispatch = useAppDispatch();
  const { items, status, error, totalCount } = useAppSelector((s) => s.medications.catalog);
  const pendingIds = useAppSelector((s) => s.cart.pendingMedicationIds);

  const pageSize = 9;
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));

  useEffect(() => {
    let sortBy: 'name' | 'price' | 'createdAt' | undefined;
    let sortDesc: boolean | undefined;

    if (sortOption === 'price_asc') {
      sortBy = 'price';
      sortDesc = false;
    } else if (sortOption === 'price_desc') {
      sortBy = 'price';
      sortDesc = true;
    } else if (sortOption === 'popular') {
      sortBy = 'createdAt';
      sortDesc = true;
    }

    dispatch(
      fetchCatalogThunk({
        categoryId: selectedCategoryId ?? undefined,
        q: searchQuery || undefined,
        sortBy,
        sortDesc,
        page,
        pageSize,
      }),
    );
  }, [selectedCategoryId, searchQuery, sortOption, page, dispatch]);

  const handleAdd = async (medicationId: number) => {
    try {
      await dispatch(addToCartThunk({ medicationId, quantity: 1 })).unwrap();
      onOpenCart?.();
    } catch {
      /* error in slice */
    }
  };

  return (
    <section className="md:col-span-9 flex flex-col min-h-[500px]">
      {status === 'loading' && items.length === 0 && (
        <div className="text-center text-on-surface-variant py-20 flex-1">Đang tải sản phẩm...</div>
      )}

      {status === 'error' && (
        <div className="rounded-lg bg-error-container/40 border border-error/30 px-4 py-3 text-sm text-error flex-1">
          {error ?? 'Không tải được danh sách thuốc'}
        </div>
      )}

      {status !== 'loading' && items.length === 0 && (
        <div className="text-center text-on-surface-variant py-20 flex-1">
          Không có sản phẩm nào trong danh mục này.
        </div>
      )}

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6 flex-1">
        {items.map((med) => (
          <ProductCard
            key={med.id}
            med={med}
            onAdd={() => handleAdd(med.id)}
            isAdding={pendingIds.includes(med.id)}
          />
        ))}
      </div>

      {totalCount > 0 && (
        <div className="mt-10 flex items-center justify-between border-t border-outline-variant/30 pt-6">
          <p className="text-sm text-on-surface-variant">
            Hiển thị <span className="font-semibold text-on-surface">{items.length}</span> /{' '}
            <span className="font-semibold text-on-surface">{totalCount}</span> sản phẩm
          </p>
          <div className="flex items-center gap-2">
            <button
              onClick={() => onPageChange(page - 1)}
              disabled={page <= 1}
              className="w-10 h-10 rounded-full flex items-center justify-center border border-outline-variant/50 text-on-surface hover:bg-surface-container-high disabled:opacity-30 transition-colors"
            >
              <span className="material-symbols-outlined text-sm">arrow_back_ios_new</span>
            </button>
            <span className="text-sm font-semibold px-4">
              {page} / {totalPages}
            </span>
            <button
              onClick={() => onPageChange(page + 1)}
              disabled={page >= totalPages}
              className="w-10 h-10 rounded-full flex items-center justify-center border border-outline-variant/50 text-on-surface hover:bg-surface-container-high disabled:opacity-30 transition-colors"
            >
              <span className="material-symbols-outlined text-sm">arrow_forward_ios</span>
            </button>
          </div>
        </div>
      )}
    </section>
  );
}
