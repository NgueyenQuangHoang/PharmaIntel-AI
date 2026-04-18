interface CartDrawerProps {
  isOpen: boolean;
  onClose: () => void;
}

export function CartDrawer({ isOpen, onClose }: CartDrawerProps) {
  return (
    <>
      {/* Backdrop */}
      {isOpen && (
        <div 
          className="fixed inset-0 bg-black/40 z-[55] transition-opacity backdrop-blur-sm"
          onClick={onClose}
        ></div>
      )}

      {/* Drawer */}
      <div 
        className={`fixed right-0 top-0 h-full w-full sm:w-96 bg-surface-container-lowest shadow-2xl z-[60] transform transition-transform duration-300 border-l border-outline-variant/15 flex flex-col ${isOpen ? 'translate-x-0' : 'translate-x-full'}`}
      >
        <div className="p-6 border-b border-surface-container-high flex items-center justify-between">
          <div className="flex items-center gap-2">
            <span className="material-symbols-outlined text-primary" style={{ fontVariationSettings: "'FILL' 1" }}>shopping_basket</span>
            <h2 className="font-headline font-bold text-xl">Giỏ hàng</h2>
          </div>
          <button onClick={onClose} className="p-2 hover:bg-surface-container-low rounded-full transition-colors">
            <span className="material-symbols-outlined">close</span>
          </button>
        </div>
        
        <div className="flex-1 overflow-y-auto p-6 space-y-6">
          {/* Cart Item 1 */}
          <div className="flex gap-4">
            <div className="w-16 h-16 bg-surface-container-low rounded-lg p-2 flex-shrink-0">
              <img alt="Cart item" className="w-full h-full object-contain" src="https://lh3.googleusercontent.com/aida-public/AB6AXuC5oEfQNqUF0HGflpaBitpOit5TcUiwOxGNzbN82Hgqn5cjI-CJkUye92NV7ONoDIUtRiAZgLl3XMnSrcXOJ5WJNDUsFvhqlONOhvk54n9lKZrSJ0qVxX1XFUmvnX4ztKVBhDsI5iMFaDzAjeMfF7rUwLpamtaZICOva9DOMxubk-0W7OM_P9pma6OVxw1yEpNgtB_aE2CAeu1-qcChxa7_OVnyenxwFuzf-FA8roUyHJt92Lxp_gSx8MCiQq7aWeRQ_dW5EhZet5E" />
            </div>
            <div className="flex-1">
              <h4 className="font-bold text-sm">Paracetamol 500mg</h4>
              <p className="text-xs text-on-surface-variant">Số lượng: 1</p>
              <p className="text-primary font-bold mt-1">35.000đ</p>
            </div>
            <button className="text-outline hover:text-error h-fit transition-colors">
              <span className="material-symbols-outlined text-lg">delete</span>
            </button>
          </div>
          
          {/* Cart Item 2 */}
          <div className="flex gap-4">
            <div className="w-16 h-16 bg-surface-container-low rounded-lg p-2 flex-shrink-0">
              <img alt="Cart item" className="w-full h-full object-contain" src="https://lh3.googleusercontent.com/aida-public/AB6AXuBlcKSHHvHE0aqHRHBNHCAh_pc5RboP1EVR0PrgZFRIkJDtbD4GWD9pxeJSRbr5YUf1EqHmpP9YqOl8EdUPAxSnD-NzlMmTY-dFuDSP6F7J7GoyGm7_pMVBjLFNDEQDcPLxgP-iKozfVWj9e4-x6cHAzzxCVGDuzeMOtbVymUDDfXAHbIDCOY7NJnEam8Z5F9bmGy_0uFpe0pE-4R_hYE-rGPWJn2lGuMUxCKa_fELdQJcD9sfQeGJvbVSF8w45GqHySLM0RWXjFoQ" />
            </div>
            <div className="flex-1">
              <h4 className="font-bold text-sm">Vitamin C 1000mg</h4>
              <p className="text-xs text-on-surface-variant">Số lượng: 2</p>
              <p className="text-primary font-bold mt-1">170.000đ</p>
            </div>
            <button className="text-outline hover:text-error h-fit transition-colors">
              <span className="material-symbols-outlined text-lg">delete</span>
            </button>
          </div>
          
          {/* Cart Item 3 */}
          <div className="flex gap-4">
            <div className="w-16 h-16 bg-surface-container-low rounded-lg p-2 flex-shrink-0">
              <img alt="Cart item" className="w-full h-full object-contain" src="https://lh3.googleusercontent.com/aida-public/AB6AXuA16ei8PXXGuxCxpV48JOd9fhf0JYP9rN7Rhttf2UBt5CavyeG8HzMoxmHlg4optHXOFd8dPLwjLrqGP_OQbissrto2QyBxtibdKWEshkV9KppkSGqPUFYetJxD8-0pjQ1fmIARLkdsuhrV1U9rVBuspQFuEbY4aNAdKEjwKBAQqfvG9awoQl-AZuTJCAzEw0ZYdI-yd7ALGjv3p7KA47NURqr2v4O2P9G2EgWblu8v1MjcqG5wimcnrAABZRmku8bZYYJOpKGjtmY" />
            </div>
            <div className="flex-1">
              <h4 className="font-bold text-sm">Oresol 245mg</h4>
              <p className="text-xs text-on-surface-variant">Số lượng: 5</p>
              <p className="text-primary font-bold mt-1">60.000đ</p>
            </div>
            <button className="text-outline hover:text-error h-fit transition-colors">
              <span className="material-symbols-outlined text-lg">delete</span>
            </button>
          </div>
        </div>

        <div className="p-6 bg-surface-container-low border-t border-outline-variant/15 space-y-4">
          <div className="flex justify-between items-center text-on-surface-variant">
            <span>Tạm tính:</span>
            <span>265.000đ</span>
          </div>
          <div className="flex justify-between items-center text-lg font-bold">
            <span>Tổng cộng:</span>
            <span className="text-primary">265.000đ</span>
          </div>
          <button className="w-full py-4 bg-gradient-to-r from-primary to-primary-container text-on-primary rounded-full font-bold shadow-lg shadow-primary/20 hover:opacity-90 active:scale-[0.98] transition-all">
            Thanh toán ngay
          </button>
        </div>
      </div>
    </>
  );
}
