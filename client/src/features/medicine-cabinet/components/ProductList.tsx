interface ProductListProps {
  onOpenCart?: () => void;
}

export function ProductList({ onOpenCart }: ProductListProps) {
  return (
    <section className="md:col-span-9">
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
        {/* Product Card 1 */}
        <div className="glass-card rounded-xl overflow-hidden flex flex-col group transition-all hover:-translate-y-1">
          <div className="aspect-square relative bg-surface-container-low">
            <img alt="Medicine product" className="w-full h-full object-contain p-8" data-alt="..." src="https://lh3.googleusercontent.com/aida-public/AB6AXuCwiMzRPJTukQFx_9X03mgt9E1xTAVTeUFI7p6ZozhZF5QLvtU9hg5J02YUiuOFb6vsvHAwFBrnetN8IyNa8ONYnLpkeh5hyOchV--12uJhWJUVNwCy4CBYa7UpHVMZwy0kaS_NBLJnrLkyWJELLD8lV4e6B4VShEKUoSyR6mRl8Kq-lflELOU9TvyeIeg1uftP_OUeu6glcOVnuHoQV2Pxr5FnjgQ-bgj7k2hlLML9J2YBvTsWVx7s_1sln4J0zNng3S_XlLnvJtk" />
            <span className="absolute top-4 left-4 bg-secondary-container text-on-secondary-container text-xs font-bold px-3 py-1 rounded-full uppercase tracking-wider">GIẢM ĐAU</span>
          </div>
          <div className="p-6 flex flex-col flex-1">
            <h3 className="font-headline font-bold text-lg mb-1 group-hover:text-primary transition-colors">Paracetamol 500mg</h3>
            <p className="text-sm text-on-surface-variant mb-4">Vỉ 10 viên. Hạ sốt, giảm đau nhanh chóng.</p>
            <div className="mt-auto flex items-center justify-between">
              <span className="text-xl font-bold text-primary">35.000đ</span>
              <button onClick={onOpenCart} className="w-10 h-10 rounded-full bg-primary text-on-primary flex items-center justify-center active:scale-90 transition-transform shadow-md hover:opacity-90">
                <span className="material-symbols-outlined" style={{ fontVariationSettings: "'FILL' 1" }}>add_shopping_cart</span>
              </button>
            </div>
          </div>
        </div>

        {/* Product Card 2 */}
        <div className="glass-card rounded-xl overflow-hidden flex flex-col group transition-all hover:-translate-y-1">
          <div className="aspect-square relative bg-surface-container-low">
            <img alt="Medicine product" className="w-full h-full object-contain p-8" data-alt="..." src="https://lh3.googleusercontent.com/aida-public/AB6AXuDaAetzEQ6atHDszkB3CndpAfRbx8Gq9a3zucJyh75sfe-OEaZL-JaIoQ1y8ceixPVpjG6ZT6cUZ5UH22pMXK27_9fYdZeGKzqPhYpb1W3ihUqxYmw-bOlpL_SUkRsLpKxJYrIrjn2_2fdmi6jUTP5RfFvqmoCUwS9ddtoiPvH6wUOi3bQ8GZlp9Glc7fLbFfJB565oWagV6-BoftOOpseHdV9KKo3n5wxOboUjAkS6VLMy3grosxLIL1fGP9w3UemnouGpFBAEFBI" />
            <span className="absolute top-4 left-4 bg-secondary-container text-on-secondary-container text-xs font-bold px-3 py-1 rounded-full uppercase tracking-wider">VITAMIN</span>
          </div>
          <div className="p-6 flex flex-col flex-1">
            <h3 className="font-headline font-bold text-lg mb-1 group-hover:text-primary transition-colors">Vitamin C 1000mg</h3>
            <p className="text-sm text-on-surface-variant mb-4">Hộp 20 viên sủi. Tăng cường đề kháng.</p>
            <div className="mt-auto flex items-center justify-between">
              <span className="text-xl font-bold text-primary">85.000đ</span>
              <button onClick={onOpenCart} className="w-10 h-10 rounded-full bg-primary text-on-primary flex items-center justify-center active:scale-90 transition-transform shadow-md hover:opacity-90">
                <span className="material-symbols-outlined" style={{ fontVariationSettings: "'FILL' 1" }}>add_shopping_cart</span>
              </button>
            </div>
          </div>
        </div>

        {/* Product Card 3 */}
        <div className="glass-card rounded-xl overflow-hidden flex flex-col group transition-all hover:-translate-y-1">
          <div className="aspect-square relative bg-surface-container-low">
            <img alt="Medicine product" className="w-full h-full object-contain p-8" data-alt="..." src="https://lh3.googleusercontent.com/aida-public/AB6AXuBl_k85vgkJ57Htnqg8OSNSzRcBtT5cL4c1wM6W0tg2o98-oRj_vjLFfVymjp4vFcdaZZHcAp60DVLZW901-_C-dmAKQ4nvCBPy3Kr2bgW8zOoU_Kv2XCslpRgsfEyg-jRF08RbxvGQtWE-4nhVDPU0YnXkwWOoixJ6oSWrCOovM9PY-3tzBVtcUG2yJXkdN1rOH3MJUFxndyBnDb5QLWrvCjI5EgqU-W1KTH612YulWfcPikqL0xhiIVOmVwyOFhwxLP7a15HLlhI" />
            <span className="absolute top-4 left-4 bg-secondary-container text-on-secondary-container text-xs font-bold px-3 py-1 rounded-full uppercase tracking-wider">HỖ TRỢ TIÊU HÓA</span>
          </div>
          <div className="p-6 flex flex-col flex-1">
            <h3 className="font-headline font-bold text-lg mb-1 group-hover:text-primary transition-colors">Oresol 245mg</h3>
            <p className="text-sm text-on-surface-variant mb-4">Gói bột. Bù nước và điện giải.</p>
            <div className="mt-auto flex items-center justify-between">
              <span className="text-xl font-bold text-primary">12.000đ</span>
              <button onClick={onOpenCart} className="w-10 h-10 rounded-full bg-primary text-on-primary flex items-center justify-center active:scale-90 transition-transform shadow-md hover:opacity-90">
                <span className="material-symbols-outlined" style={{ fontVariationSettings: "'FILL' 1" }}>add_shopping_cart</span>
              </button>
            </div>
          </div>
        </div>

        {/* Product Card 4 */}
        <div className="glass-card rounded-xl overflow-hidden flex flex-col group transition-all hover:-translate-y-1">
          <div className="aspect-square relative bg-surface-container-low">
            <img alt="Medicine product" className="w-full h-full object-contain p-8" data-alt="..." src="https://lh3.googleusercontent.com/aida-public/AB6AXuDG8xTRhc2RI1uN3Yscqk6MwXZg9V0moYI2lA1FcWqTFUez2U-RSW1y6eCV-zjRKapVF4HsJVWupEp8WdsWSM1_8KRVBFS4Vjon9qI9ldp-cAOtdgfi10gk3EDMBDj-Kb7tTvCaRyfXSS6AW5Q5oZOwUX1Z2TgIK1oSuX7amXn5KfegWiDZCkc2YK0OUaNoWKQKf9QwNOQ3TLU_NwEQXuK1xcllXpHvEmenmH487yI9cW45u_SH0LQi81zCQ4cuxZxEr9moEqkTjyE" />
            <span className="absolute top-4 left-4 bg-secondary-container text-on-secondary-container text-xs font-bold px-3 py-1 rounded-full uppercase tracking-wider">KHÁNG SINH</span>
          </div>
          <div className="p-6 flex flex-col flex-1">
            <h3 className="font-headline font-bold text-lg mb-1 group-hover:text-primary transition-colors">Amoxicillin 500mg</h3>
            <p className="text-sm text-on-surface-variant mb-4">Hộp 2 vỉ x 10 viên. Điều trị nhiễm khuẩn.</p>
            <div className="mt-auto flex items-center justify-between">
              <span className="text-xl font-bold text-primary">125.000đ</span>
              <button onClick={onOpenCart} className="w-10 h-10 rounded-full bg-primary text-on-primary flex items-center justify-center active:scale-90 transition-transform shadow-md hover:opacity-90">
                <span className="material-symbols-outlined" style={{ fontVariationSettings: "'FILL' 1" }}>add_shopping_cart</span>
              </button>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
