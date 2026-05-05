import { useState } from 'react';
import { MedicineHeader } from '@/features/medicine-cabinet/components/MedicineHeader';
import { MedicineSidebar } from '@/features/medicine-cabinet/components/MedicineSidebar';
import { ProductList } from '@/features/medicine-cabinet/components/ProductList';
import { CartDrawer } from '@/features/medicine-cabinet/components/CartDrawer';

export function MedicineCabinetPage() {
  const [isCartOpen, setIsCartOpen] = useState(false);
  const [selectedCategoryId, setSelectedCategoryId] = useState<number | null>(null);

  return (
    <div className="pt-8 pb-20 px-4 md:px-8 max-w-7xl mx-auto animate-in fade-in zoom-in-95 duration-500">
      <MedicineHeader />

      <div className="grid grid-cols-1 md:grid-cols-12 gap-10">
        <MedicineSidebar
          selectedCategoryId={selectedCategoryId}
          onSelectCategory={setSelectedCategoryId}
        />
        <ProductList
          selectedCategoryId={selectedCategoryId}
          onOpenCart={() => setIsCartOpen(true)}
        />
      </div>

      {/* Floating Action Button */}
      <div className="fixed bottom-8 right-8 z-40 md:hidden">
        <button
          onClick={() => setIsCartOpen(true)}
          className="w-16 h-16 rounded-full bg-primary text-on-primary shadow-2xl flex items-center justify-center active:scale-95 transition-all"
        >
          <span className="material-symbols-outlined text-3xl">shopping_cart</span>
        </button>
      </div>

      <CartDrawer isOpen={isCartOpen} onClose={() => setIsCartOpen(false)} />
    </div>
  );
}
