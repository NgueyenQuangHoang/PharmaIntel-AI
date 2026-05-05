import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppSelector } from '@/hooks/redux';
import { formatVnd } from '@/utils/format';

const SHIPPING_FEE = 30000;
const EXPRESS_SHIPPING_FEE = 55000;

export function CheckoutPage() {
  const navigate = useNavigate();
  const cart = useAppSelector((s) => s.cart.cart);
  const items = cart?.items ?? [];

  const [deliveryMethod, setDeliveryMethod] = useState<'standard' | 'express'>('standard');
  const [paymentMethod, setPaymentMethod] = useState<'credit_card' | 'momo' | 'bank_transfer'>('credit_card');

  const shippingFee = deliveryMethod === 'standard' ? SHIPPING_FEE : EXPRESS_SHIPPING_FEE;
  
  // Calculate total with selected shipping fee
  const orderTotal = cart ? cart.total + shippingFee : 0;

  if (!cart || items.length === 0) {
    return (
      <div className="pt-12 pb-24 px-4 md:px-12 max-w-7xl mx-auto flex flex-col items-center justify-center min-h-[50vh]">
        <span className="material-symbols-outlined text-6xl text-outline mb-4">
          shopping_basket
        </span>
        <h2 className="text-2xl font-bold mb-2">Giỏ hàng trống</h2>
        <p className="text-on-surface-variant mb-6 text-center">
          Vui lòng thêm sản phẩm vào giỏ hàng trước khi thanh toán.
        </p>
        <button 
          onClick={() => navigate('/medicine')}
          className="px-6 py-3 bg-primary text-on-primary rounded-lg font-bold hover:bg-primary/90 transition-colors"
        >
          Trở lại Tủ thuốc
        </button>
      </div>
    );
  }

  return (
    <div className="pt-12 pb-24 px-4 md:px-12 max-w-7xl mx-auto animate-in fade-in zoom-in-95 duration-500">
      <header className="mb-12">
        <h1 className="text-4xl md:text-5xl font-extrabold tracking-tight text-on-surface mb-2 font-headline">Thanh toán</h1>
        <p className="text-on-surface-variant font-medium">Hoàn tất đơn hàng của bạn với bảo mật cấp độ y tế.</p>
      </header>

      <div className="flex flex-col lg:flex-row gap-12">
        {/* Left Side: Transactional Forms */}
        <div className="flex-1 space-y-12">
          {/* Shipping Info Section */}
          <section>
            <div className="flex items-center gap-3 mb-6">
              <span className="material-symbols-outlined text-primary" style={{ fontVariationSettings: "'FILL' 1" }}>local_shipping</span>
              <h2 className="text-2xl font-bold tracking-tight font-headline">Thông tin giao hàng</h2>
            </div>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6 bg-surface-container-low p-8 rounded-xl">
              <div className="space-y-2">
                <label className="text-sm font-semibold text-on-surface-variant px-1">Họ và tên</label>
                <input className="w-full h-12 px-4 rounded-lg bg-surface-container-lowest border-none focus:ring-2 focus:ring-primary transition-all outline-none" placeholder="Nguyễn Văn A" type="text"/>
              </div>
              <div className="space-y-2">
                <label className="text-sm font-semibold text-on-surface-variant px-1">Số điện thoại</label>
                <input className="w-full h-12 px-4 rounded-lg bg-surface-container-lowest border-none focus:ring-2 focus:ring-primary transition-all outline-none" placeholder="090 1234 567" type="tel"/>
              </div>
              <div className="md:col-span-2 space-y-2">
                <label className="text-sm font-semibold text-on-surface-variant px-1">Địa chỉ nhận hàng</label>
                <input className="w-full h-12 px-4 rounded-lg bg-surface-container-lowest border-none focus:ring-2 focus:ring-primary transition-all outline-none" placeholder="Số nhà, Tên đường, Phường/Xã..." type="text"/>
              </div>
              <div className="space-y-2">
                <label className="text-sm font-semibold text-on-surface-variant px-1">Tỉnh/Thành phố</label>
                <select className="w-full h-12 px-4 rounded-lg bg-surface-container-lowest border-none focus:ring-2 focus:ring-primary transition-all appearance-none outline-none">
                  <option>Hồ Chí Minh</option>
                  <option>Hà Nội</option>
                  <option>Đà Nẵng</option>
                </select>
              </div>
              <div className="space-y-2">
                <label className="text-sm font-semibold text-on-surface-variant px-1">Quận/Huyện</label>
                <select className="w-full h-12 px-4 rounded-lg bg-surface-container-lowest border-none focus:ring-2 focus:ring-primary transition-all appearance-none outline-none">
                  <option>Quận 1</option>
                  <option>Quận 3</option>
                  <option>Quận Bình Thạnh</option>
                </select>
              </div>
            </div>
          </section>

          {/* Delivery Method Section */}
          <section>
            <div className="flex items-center gap-3 mb-6">
              <span className="material-symbols-outlined text-primary" style={{ fontVariationSettings: "'FILL' 1" }}>package_2</span>
              <h2 className="text-2xl font-bold tracking-tight font-headline">Phương thức vận chuyển</h2>
            </div>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <label 
                className={`relative flex items-center p-6 border-2 rounded-xl cursor-pointer transition-all ${
                  deliveryMethod === 'standard' 
                    ? 'bg-surface-container-lowest border-primary' 
                    : 'bg-surface-container-low border-transparent hover:border-outline-variant'
                }`}
                onClick={() => setDeliveryMethod('standard')}
              >
                <input 
                  className="hidden" 
                  name="delivery" 
                  type="radio" 
                  checked={deliveryMethod === 'standard'} 
                  readOnly 
                />
                <div className="flex-1">
                  <div className="font-bold text-lg">Giao hàng tiêu chuẩn</div>
                  <div className="text-on-surface-variant text-sm">Dự kiến nhận hàng: 3-5 ngày</div>
                </div>
                <div className="font-bold text-primary">{formatVnd(SHIPPING_FEE)}</div>
                {deliveryMethod === 'standard' && (
                  <div className="absolute top-4 right-4 text-primary">
                    <span className="material-symbols-outlined" style={{ fontVariationSettings: "'FILL' 1" }}>check_circle</span>
                  </div>
                )}
              </label>
              
              <label 
                className={`relative flex items-center p-6 border-2 rounded-xl cursor-pointer transition-all ${
                  deliveryMethod === 'express' 
                    ? 'bg-surface-container-lowest border-primary' 
                    : 'bg-surface-container-low border-transparent hover:border-outline-variant'
                }`}
                onClick={() => setDeliveryMethod('express')}
              >
                <input 
                  className="hidden" 
                  name="delivery" 
                  type="radio" 
                  checked={deliveryMethod === 'express'} 
                  readOnly 
                />
                <div className="flex-1">
                  <div className="font-bold text-lg">Giao hàng hỏa tốc</div>
                  <div className="text-on-surface-variant text-sm">Dự kiến nhận hàng: 24h</div>
                </div>
                <div className="font-bold text-on-surface">{formatVnd(EXPRESS_SHIPPING_FEE)}</div>
                {deliveryMethod === 'express' && (
                  <div className="absolute top-4 right-4 text-primary">
                    <span className="material-symbols-outlined" style={{ fontVariationSettings: "'FILL' 1" }}>check_circle</span>
                  </div>
                )}
              </label>
            </div>
          </section>

          {/* Payment Method Section */}
          <section>
            <div className="flex items-center gap-3 mb-6">
              <span className="material-symbols-outlined text-primary" style={{ fontVariationSettings: "'FILL' 1" }}>account_balance_wallet</span>
              <h2 className="text-2xl font-bold tracking-tight font-headline">Phương thức thanh toán</h2>
            </div>
            <div className="space-y-3">
              <div 
                className={`p-4 rounded-xl border flex items-center gap-4 cursor-pointer transition-all ${
                  paymentMethod === 'credit_card' 
                    ? 'bg-primary-fixed/20 border-primary' 
                    : 'bg-surface-container-lowest border-outline-variant/30 hover:bg-surface-container-low'
                }`}
                onClick={() => setPaymentMethod('credit_card')}
              >
                <div className="w-12 h-12 rounded-lg bg-surface-container flex items-center justify-center">
                  <span className="material-symbols-outlined text-primary">credit_card</span>
                </div>
                <div className="flex-1 font-semibold text-lg">Thẻ tín dụng / Ghi nợ (Visa/Mastercard)</div>
                <input 
                  className="w-5 h-5 text-primary border-outline-variant focus:ring-primary cursor-pointer" 
                  name="payment" 
                  type="radio" 
                  checked={paymentMethod === 'credit_card'}
                  readOnly
                />
              </div>

              <div 
                className={`p-4 rounded-xl border flex items-center gap-4 cursor-pointer transition-all ${
                  paymentMethod === 'momo' 
                    ? 'bg-primary-fixed/20 border-primary' 
                    : 'bg-surface-container-lowest border-outline-variant/30 hover:bg-surface-container-low'
                }`}
                onClick={() => setPaymentMethod('momo')}
              >
                <div className="w-12 h-12 rounded-lg bg-surface-container flex items-center justify-center overflow-hidden">
                  <div className="w-full h-full bg-pink-600 flex items-center justify-center text-white font-bold text-xs">MoMo</div>
                </div>
                <div className="flex-1 font-semibold text-lg">Ví điện tử MoMo</div>
                <input 
                  className="w-5 h-5 text-primary border-outline-variant focus:ring-primary cursor-pointer" 
                  name="payment" 
                  type="radio" 
                  checked={paymentMethod === 'momo'}
                  readOnly
                />
              </div>

              <div 
                className={`p-4 rounded-xl border flex items-center gap-4 cursor-pointer transition-all ${
                  paymentMethod === 'bank_transfer' 
                    ? 'bg-primary-fixed/20 border-primary' 
                    : 'bg-surface-container-lowest border-outline-variant/30 hover:bg-surface-container-low'
                }`}
                onClick={() => setPaymentMethod('bank_transfer')}
              >
                <div className="w-12 h-12 rounded-lg bg-surface-container flex items-center justify-center">
                  <span className="material-symbols-outlined text-primary">account_balance</span>
                </div>
                <div className="flex-1 font-semibold text-lg">Chuyển khoản ngân hàng</div>
                <input 
                  className="w-5 h-5 text-primary border-outline-variant focus:ring-primary cursor-pointer" 
                  name="payment" 
                  type="radio" 
                  checked={paymentMethod === 'bank_transfer'}
                  readOnly
                />
              </div>
            </div>
          </section>
        </div>

        {/* Right Side: Sticky Order Summary */}
        <aside className="lg:w-[400px]">
          <div className="sticky top-32 bg-surface-container-lowest rounded-2xl shadow-xl shadow-slate-200/50 p-8 border border-outline-variant/10 overflow-hidden">
            <div className="absolute top-0 left-0 w-1 bg-gradient-to-b from-primary to-secondary h-full"></div>
            <h3 className="text-xl font-bold mb-8 font-headline">Tóm tắt đơn hàng</h3>
            
            {/* Product List */}
            <div className="space-y-6 mb-8 max-h-[300px] overflow-y-auto pr-2 custom-scrollbar">
              {items.map((item) => (
                <div key={item.id} className="flex gap-4">
                  <div className="w-20 h-20 bg-surface-container-low rounded-lg p-2 shrink-0 flex items-center justify-center">
                    {item.imageUrl ? (
                      <img 
                        alt={item.name} 
                        className="w-full h-full object-contain" 
                        src={item.imageUrl}
                      />
                    ) : (
                      <span className="material-symbols-outlined text-outline">medication</span>
                    )}
                  </div>
                  <div className="flex-1 min-w-0">
                    <h4 className="font-bold text-sm leading-tight mb-1 truncate" title={item.name}>{item.name}</h4>
                    <div className="text-xs text-on-surface-variant mb-2">Số lượng: {item.quantity}</div>
                    <div className="font-bold text-primary">{formatVnd(item.lineTotal)}</div>
                  </div>
                </div>
              ))}
            </div>

            {/* Pricing Details */}
            <div className="space-y-4 border-t border-outline-variant/20 pt-8 mb-8">
              <div className="flex justify-between text-on-surface-variant">
                <span>Tạm tính</span>
                <span>{formatVnd(cart.subtotal)}</span>
              </div>
              <div className="flex justify-between text-on-surface-variant">
                <span>Phí vận chuyển</span>
                <span>{formatVnd(shippingFee)}</span>
              </div>
              <div className="flex justify-between text-on-surface-variant">
                <span>Giảm giá</span>
                <span className="text-error">-{formatVnd(cart.totalDiscount)}</span>
              </div>
              
              <div className="flex justify-between items-end pt-4 border-t border-outline-variant/10">
                <span className="font-bold text-lg">Tổng cộng</span>
                <div className="text-right">
                  <div className="text-xs text-on-surface-variant font-medium">(Đã bao gồm thuế VAT)</div>
                  <div className="text-3xl font-extrabold text-primary tracking-tighter">{formatVnd(orderTotal)}</div>
                </div>
              </div>
            </div>

            {/* Promo Code Input */}
            <div className="flex gap-2 mb-8">
              <input className="flex-1 h-11 px-4 rounded-lg bg-surface-container-low border-none text-sm outline-none focus:ring-2 focus:ring-primary" placeholder="Mã giảm giá" type="text"/>
              <button className="px-4 bg-surface-container-highest text-on-surface font-bold rounded-lg text-sm hover:bg-outline-variant transition-colors">Áp dụng</button>
            </div>

            <button 
              className="w-full py-5 bg-gradient-to-r from-primary to-primary-container text-on-primary font-extrabold text-xl rounded-full shadow-lg shadow-primary/20 hover:scale-[1.02] active:scale-95 transition-all flex items-center justify-center gap-3 disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:scale-100"
              disabled={cart.hasUnavailableItems}
              onClick={() => alert('Chức năng đặt hàng đang được phát triển!')}
            >
              Thanh toán ngay
              <span className="material-symbols-outlined">arrow_forward</span>
            </button>
            
            {cart.hasPrescriptionRequired && (
              <p className="mt-3 text-xs text-error font-semibold text-center">
                Đơn hàng có thuốc cần kê đơn. Dược sĩ sẽ liên hệ xác nhận.
              </p>
            )}

            <div className="mt-6 flex items-center justify-center gap-2 text-xs text-on-surface-variant font-medium">
              <span className="material-symbols-outlined text-[16px]">lock</span>
              Giao dịch bảo mật 256-bit SSL
            </div>
          </div>
        </aside>
      </div>
    </div>
  );
}
