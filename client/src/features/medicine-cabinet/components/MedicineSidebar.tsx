import { useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '@/hooks/redux';
import { fetchCategoriesThunk } from '@/features/categories/categories-slice';

interface MedicineSidebarProps {
  selectedCategoryId: number | null;
  onSelectCategory: (categoryId: number | null) => void;
}

export function MedicineSidebar({ selectedCategoryId, onSelectCategory }: MedicineSidebarProps) {
  const dispatch = useAppDispatch();
  const { items, status } = useAppSelector((s) => s.categories);

  useEffect(() => {
    if (status === 'idle') {
      dispatch(fetchCategoriesThunk());
    }
  }, [status, dispatch]);

  return (
    <aside className="md:col-span-3">
      {/* Desktop View */}
      <div className="hidden md:block space-y-8">
        <div>
          <h3 className="text-xs font-bold tracking-widest text-outline uppercase mb-6">DANH MỤC SẢN PHẨM</h3>
          <ul className="space-y-2">
            <li>
              <button
                onClick={() => onSelectCategory(null)}
                className={`w-full flex items-center justify-between px-4 py-3 rounded-lg font-semibold transition-colors ${
                  selectedCategoryId === null
                    ? 'bg-primary-fixed text-on-primary-fixed-variant'
                    : 'text-on-surface-variant hover:bg-surface-container-high'
                }`}
              >
                <span>Tất cả sản phẩm</span>
                <span className="material-symbols-outlined text-sm">chevron_right</span>
              </button>
            </li>
            {status === 'loading' && (
              <li className="px-4 py-3 text-sm text-on-surface-variant">Đang tải...</li>
            )}
            {status === 'error' && (
              <li className="px-4 py-3 text-sm text-error">Không tải được danh mục</li>
            )}
            {items.map((cat) => (
              <li key={cat.id}>
                <button
                  onClick={() => onSelectCategory(cat.id)}
                  className={`w-full flex items-center justify-between px-4 py-3 rounded-lg transition-colors ${
                    selectedCategoryId === cat.id
                      ? 'bg-primary-fixed text-on-primary-fixed-variant font-semibold'
                      : 'text-on-surface-variant hover:bg-surface-container-high'
                  }`}
                >
                  <span className="uppercase">{cat.name}</span>
                  <span className="material-symbols-outlined text-sm">chevron_right</span>
                </button>
              </li>
            ))}
          </ul>
        </div>
        <div className="p-6 rounded-xl bg-gradient-to-br from-primary to-primary-container text-on-primary shadow-lg">
          <span className="material-symbols-outlined mb-4 text-secondary-fixed" style={{ fontVariationSettings: "'FILL' 1" }}>clinical_notes</span>
          <h4 className="font-headline font-bold mb-2">Tư vấn AI miễn phí</h4>
          <p className="text-sm text-on-primary/80 mb-4">Bạn không chắc nên mua loại nào? Hãy để AI giúp bạn phân tích triệu chứng.</p>
          <Link
            to="/diagnostic"
            className="block w-full py-2 bg-surface-container-lowest text-primary hover:bg-white font-bold rounded-full text-sm text-center transition-colors"
          >
            Bắt đầu tư vấn
          </Link>
        </div>
      </div>

      {/* Mobile View: Horizontal Scrollable Chips */}
      <div className="md:hidden w-full mb-6">
        <h3 className="text-xs font-bold tracking-widest text-outline uppercase mb-3">DANH MỤC SẢN PHẨM</h3>
        <div className="flex gap-2 overflow-x-auto pb-3 -mx-4 px-4 scrollbar-none snap-x snap-mandatory">
          <button
            onClick={() => onSelectCategory(null)}
            className={`px-4 py-2.5 rounded-full text-sm font-semibold transition-all whitespace-nowrap snap-start border ${
              selectedCategoryId === null
                ? 'bg-primary text-on-primary border-primary shadow-sm shadow-primary/20'
                : 'bg-surface-container-low text-on-surface-variant border-outline-variant/30 hover:bg-surface-container-high'
            }`}
          >
            Tất cả
          </button>
          {items.map((cat) => {
            const active = selectedCategoryId === cat.id;
            return (
              <button
                key={cat.id}
                onClick={() => onSelectCategory(cat.id)}
                className={`px-4 py-2.5 rounded-full text-sm font-semibold transition-all whitespace-nowrap snap-start border ${
                  active
                    ? 'bg-primary text-on-primary border-primary shadow-sm shadow-primary/20'
                    : 'bg-surface-container-low text-on-surface-variant border-outline-variant/30 hover:bg-surface-container-high'
                }`}
              >
                {cat.name}
              </button>
            );
          })}
        </div>
      </div>
    </aside>
  );
}
