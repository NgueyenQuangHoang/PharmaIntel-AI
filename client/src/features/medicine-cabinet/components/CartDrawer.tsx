import { useAppDispatch, useAppSelector } from '@/hooks/redux';
import {
  removeCartItemThunk,
  updateCartItemThunk,
} from '@/features/cart/cart-slice';
import { formatVnd } from '@/utils/format';

interface CartDrawerProps {
  isOpen: boolean;
  onClose: () => void;
}

export function CartDrawer({ isOpen, onClose }: CartDrawerProps) {
  const dispatch = useAppDispatch();
  const cart = useAppSelector((s) => s.cart.cart);
  const status = useAppSelector((s) => s.cart.status);
  const pendingIds = useAppSelector((s) => s.cart.pendingMedicationIds);

  const items = cart?.items ?? [];

  const handleQty = (medicationId: number, nextQty: number) => {
    if (nextQty <= 0) {
      dispatch(removeCartItemThunk(medicationId));
    } else {
      dispatch(updateCartItemThunk({ medicationId, quantity: nextQty }));
    }
  };

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
        className={`fixed right-0 top-0 h-full w-full sm:w-96 bg-surface-container-lowest shadow-2xl z-[60] transform transition-transform duration-300 border-l border-outline-variant/15 flex flex-col ${
          isOpen ? 'translate-x-0' : 'translate-x-full'
        }`}
      >
        <div className="p-6 border-b border-surface-container-high flex items-center justify-between">
          <div className="flex items-center gap-2">
            <span
              className="material-symbols-outlined text-primary"
              style={{ fontVariationSettings: "'FILL' 1" }}
            >
              shopping_basket
            </span>
            <h2 className="font-headline font-bold text-xl">Giỏ hàng</h2>
            {cart && cart.totalItems > 0 && (
              <span className="text-sm text-on-surface-variant">({cart.totalItems})</span>
            )}
          </div>
          <button
            onClick={onClose}
            className="p-2 hover:bg-surface-container-low rounded-full transition-colors"
          >
            <span className="material-symbols-outlined">close</span>
          </button>
        </div>

        <div className="flex-1 overflow-y-auto p-6 space-y-6">
          {status === 'loading' && items.length === 0 && (
            <p className="text-sm text-on-surface-variant text-center py-10">Đang tải...</p>
          )}

          {status !== 'loading' && items.length === 0 && (
            <div className="flex flex-col items-center justify-center py-16 text-center">
              <span className="material-symbols-outlined text-6xl text-outline mb-4">
                shopping_basket
              </span>
              <p className="text-on-surface-variant">Giỏ hàng đang trống</p>
            </div>
          )}

          {items.map((item) => {
            const isPending = pendingIds.includes(item.medicationId);
            return (
              <div key={item.id} className="flex gap-4">
                <div className="w-16 h-16 bg-surface-container-low rounded-lg p-2 flex-shrink-0">
                  {item.imageUrl ? (
                    <img
                      alt={item.name}
                      className="w-full h-full object-contain"
                      src={item.imageUrl}
                    />
                  ) : (
                    <div className="w-full h-full flex items-center justify-center">
                      <span className="material-symbols-outlined text-outline">medication</span>
                    </div>
                  )}
                </div>
                <div className="flex-1 min-w-0">
                  <h4 className="font-bold text-sm truncate">{item.name}</h4>
                  <p className="text-primary font-bold mt-1">{formatVnd(item.lineTotal)}</p>
                  <div className="flex items-center gap-2 mt-2">
                    <button
                      onClick={() => handleQty(item.medicationId, item.quantity - 1)}
                      disabled={isPending}
                      className="w-7 h-7 rounded-full border border-outline-variant flex items-center justify-center hover:bg-surface-container-low disabled:opacity-50"
                    >
                      <span className="material-symbols-outlined text-sm">remove</span>
                    </button>
                    <span className="text-sm font-semibold min-w-[24px] text-center">
                      {item.quantity}
                    </span>
                    <button
                      onClick={() => handleQty(item.medicationId, item.quantity + 1)}
                      disabled={isPending || item.quantity >= item.stockQuantity}
                      className="w-7 h-7 rounded-full border border-outline-variant flex items-center justify-center hover:bg-surface-container-low disabled:opacity-50"
                    >
                      <span className="material-symbols-outlined text-sm">add</span>
                    </button>
                  </div>
                  {!item.isAvailable && (
                    <p className="text-xs text-error mt-1">Sản phẩm hiện không khả dụng</p>
                  )}
                </div>
                <button
                  onClick={() => dispatch(removeCartItemThunk(item.medicationId))}
                  disabled={isPending}
                  className="text-outline hover:text-error h-fit transition-colors disabled:opacity-50"
                >
                  <span className="material-symbols-outlined text-lg">delete</span>
                </button>
              </div>
            );
          })}
        </div>

        {cart && items.length > 0 && (
          <div className="p-6 bg-surface-container-low border-t border-outline-variant/15 space-y-4">
            <div className="flex justify-between items-center text-on-surface-variant">
              <span>Tạm tính:</span>
              <span>{formatVnd(cart.subtotal)}</span>
            </div>
            {cart.totalDiscount > 0 && (
              <div className="flex justify-between items-center text-secondary">
                <span>Giảm giá:</span>
                <span>-{formatVnd(cart.totalDiscount)}</span>
              </div>
            )}
            <div className="flex justify-between items-center text-lg font-bold">
              <span>Tổng cộng:</span>
              <span className="text-primary">{formatVnd(cart.total)}</span>
            </div>
            <button
              disabled={cart.hasUnavailableItems}
              className="w-full py-4 bg-gradient-to-r from-primary to-primary-container text-on-primary rounded-full font-bold shadow-lg shadow-primary/20 hover:opacity-90 active:scale-[0.98] transition-all disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Thanh toán ngay
            </button>
            {cart.hasPrescriptionRequired && (
              <p className="text-xs text-on-surface-variant text-center">
                Có sản phẩm cần đơn thuốc khi thanh toán.
              </p>
            )}
          </div>
        )}
      </div>
    </>
  );
}
