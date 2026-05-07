import { useState, useEffect } from 'react';
import { MedicineHeader } from '@/features/medicine-cabinet/components/MedicineHeader';
import { MedicineSidebar } from '@/features/medicine-cabinet/components/MedicineSidebar';
import { ProductList } from '@/features/medicine-cabinet/components/ProductList';
import { useAppDispatch } from '@/hooks/redux';
import { openCart } from '@/features/cart/cart-slice';

export function MedicineCabinetPage() {
  const dispatch = useAppDispatch();
  const [selectedCategoryId, setSelectedCategoryId] = useState<number | null>(null);
  
  const [searchInput, setSearchInput] = useState('');
  const [search, setSearch] = useState('');
  const [sortOption, setSortOption] = useState('popular');
  const [page, setPage] = useState(1);

  // Reset page khi search/category/sort doi.
  // Day la pattern reset state co chu y - khong phai cascading render khong kiem soat.
  // Disable rule react-hooks/set-state-in-effect cho dong nay vi day la use case
  // hop le (dong bo state dieu khien voi state filter).
  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setPage(1);
  }, [search, selectedCategoryId, sortOption]);

  useEffect(() => {
    const t = setTimeout(() => {
      const q = searchInput.trim();
      if (search !== q) {
        setSearch(q);
      }
    }, 300);
    return () => clearTimeout(t);
  }, [searchInput, search]);

  return (
    <div className="pt-8 pb-20 px-4 md:px-8 max-w-7xl mx-auto animate-in fade-in zoom-in-95 duration-500">
      <MedicineHeader 
        searchInput={searchInput}
        onSearchChange={setSearchInput}
        sortOption={sortOption}
        onSortChange={setSortOption}
      />

      <div className="grid grid-cols-1 md:grid-cols-12 gap-10">
        <MedicineSidebar
          selectedCategoryId={selectedCategoryId}
          onSelectCategory={setSelectedCategoryId}
        />
        <ProductList
          selectedCategoryId={selectedCategoryId}
          searchQuery={search}
          sortOption={sortOption}
          page={page}
          onPageChange={setPage}
          onOpenCart={() => dispatch(openCart())}
        />
      </div>

      {/* Floating Action Button */}
      <div className="fixed bottom-8 right-8 z-40 md:hidden">
        <button
          onClick={() => dispatch(openCart())}
          className="w-16 h-16 rounded-full bg-primary text-on-primary shadow-2xl flex items-center justify-center active:scale-95 transition-all"
        >
          <span className="material-symbols-outlined text-3xl">shopping_cart</span>
        </button>
      </div>
    </div>
  );
}
