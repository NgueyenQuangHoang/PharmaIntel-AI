// =============================================================================
// CheckoutPage - Trang thanh toan
// Flow: load saved addresses -> chon hoac them moi -> submit -> POST /orders/checkout
//       -> backend tu dam bao COD PaymentMethod -> redirect /orders/:id
// MVP: chi COD; Express shipping cosmetic (backend van flat 30k).
// =============================================================================
import { useEffect, useState, useRef } from 'react'
import { useNavigate } from 'react-router-dom'
import axios from 'axios'
import { useAppDispatch, useAppSelector } from '@/hooks/redux'
import { formatVnd } from '@/utils/format'
import { addressesApi } from '@/features/addresses/addresses-api'
import type { AddressDto } from '@/features/addresses/types'
import { ordersApi } from '@/features/orders/orders-api'
import type { CheckoutPaymentType } from '@/features/orders/types'
import { fetchCartThunk } from '@/features/cart/cart-slice'
import { prescriptionsApi } from '@/features/prescriptions/prescriptions-api'
import type { PrescriptionListItem } from '@/features/prescriptions/types'
import { PrescriptionPicker } from '@/features/prescriptions/components/PrescriptionPicker'

const SHIPPING_FEE = 30000
const EXPRESS_SHIPPING_FEE = 55000

// Shape tu provinces.open-api.vn (chi cac field minh dung)
type VnProvince = { code: number; name: string }
type VnWard = { code: number; name: string }

type NewAddressForm = {
  recipientName: string
  phone: string
  province: string
  ward: string
  streetAddress: string
}

const EMPTY_ADDRESS: NewAddressForm = {
  recipientName: '',
  phone: '',
  province: '',
  ward: '',
  streetAddress: '',
}

type FieldErrors = Partial<Record<keyof NewAddressForm, string>>

function validateAddressForm(form: NewAddressForm): FieldErrors {
  const errors: FieldErrors = {}
  if (form.recipientName.trim().length < 2) errors.recipientName = 'Họ tên tối thiểu 2 ký tự'
  // VN phone: 0xxxxxxxxx (10) hoac +84xxxxxxxxx (12). Remove space truoc khi check.
  const phone = form.phone.replace(/\s/g, '')
  if (!/^(0|\+84)[0-9]{9,10}$/.test(phone)) errors.phone = 'Số điện thoại không hợp lệ'
  if (!form.province.trim()) errors.province = 'Vui lòng chọn tỉnh/thành phố'
  if (!form.ward.trim()) errors.ward = 'Vui lòng chọn phường/xã'
  if (form.streetAddress.trim().length < 5) errors.streetAddress = 'Địa chỉ tối thiểu 5 ký tự'
  return errors
}

function extractApiError(err: unknown, fallback: string): string {
  if (axios.isAxiosError(err)) {
    const data = err.response?.data as
      | { title?: string; detail?: string; message?: string; errors?: Record<string, string[]> }
      | undefined
    if (data?.errors) {
      const first = Object.values(data.errors).flat()[0]
      if (first) return first
    }
    return data?.detail ?? data?.title ?? data?.message ?? err.message ?? fallback
  }
  return fallback
}

// -----------------------------------------------------------------------------
// Searchable Dropdown Component
// -----------------------------------------------------------------------------
function SearchableDropdown({
  value,
  onChange,
  options,
  placeholder,
  disabled
}: {
  value: string
  onChange: (v: string) => void
  options: { label: string; value: string }[]
  placeholder: string
  disabled?: boolean
}) {
  const [open, setOpen] = useState(false)
  const [search, setSearch] = useState('')
  const ref = useRef<HTMLDivElement>(null)

  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) {
        setOpen(false)
      }
    }
    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [])

  const filtered = options.filter(o => 
    o.label.toLowerCase().includes(search.toLowerCase())
  )

  return (
    <div className="relative" ref={ref}>
      <button
        type="button"
        disabled={disabled}
        onClick={() => setOpen(!open)}
        className="w-full h-12 px-4 rounded-lg bg-surface-container-lowest border-none focus:ring-2 focus:ring-primary transition-all outline-none disabled:opacity-50 flex items-center justify-between text-left"
      >
        <span className={value ? 'text-on-surface' : 'text-on-surface-variant'}>
          {value || placeholder}
        </span>
        <span className="material-symbols-outlined text-on-surface-variant">
          arrow_drop_down
        </span>
      </button>

      {open && (
        <div className="absolute z-50 w-full mt-1 bg-surface-container-lowest rounded-lg shadow-xl shadow-slate-200/50 border border-outline-variant/10 overflow-hidden">
          <div className="p-2 border-b border-outline-variant/10">
            <input
              type="text"
              autoFocus
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Tìm kiếm..."
              className="w-full h-10 px-3 bg-surface-container-low rounded-md border-none outline-none focus:ring-2 focus:ring-primary/50 text-sm"
            />
          </div>
          <div className="max-h-[200px] overflow-y-auto custom-scrollbar">
            {filtered.length === 0 ? (
              <div className="p-3 text-sm text-center text-on-surface-variant">Không tìm thấy</div>
            ) : (
              filtered.map(o => (
                <button
                  key={o.value}
                  type="button"
                  onClick={() => {
                    onChange(o.value)
                    setOpen(false)
                    setSearch('')
                  }}
                  className={`w-full text-left px-4 py-2.5 text-sm hover:bg-surface-container-low transition-colors ${
                    value === o.value ? 'bg-primary/10 text-primary font-medium' : 'text-on-surface'
                  }`}
                >
                  {o.label}
                </button>
              ))
            )}
          </div>
        </div>
      )}
    </div>
  )
}

export function CheckoutPage() {
  const navigate = useNavigate()
  const dispatch = useAppDispatch()
  const cart = useAppSelector((s) => s.cart.cart)
  const items = cart?.items ?? []

  const [addresses, setAddresses] = useState<AddressDto[]>([])
  const [addressesLoading, setAddressesLoading] = useState(true)
  const [selectedAddressId, setSelectedAddressId] = useState<number | null>(null)
  const [showNewAddressForm, setShowNewAddressForm] = useState(false)
  const [newAddress, setNewAddress] = useState<NewAddressForm>(EMPTY_ADDRESS)
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({})

  const [provinces, setProvinces] = useState<VnProvince[]>([])
  const [wards, setWards] = useState<VnWard[]>([])
  const [provincesLoading, setProvincesLoading] = useState(true)
  const [wardsLoading, setWardsLoading] = useState(false)

  const [deliveryMethod, setDeliveryMethod] = useState<'standard' | 'express'>('standard')
  const [paymentType, setPaymentType] = useState<CheckoutPaymentType>('cod')
  const [submitting, setSubmitting] = useState(false)
  const [submitError, setSubmitError] = useState<string | null>(null)

  const [verifiedPrescriptions, setVerifiedPrescriptions] = useState<PrescriptionListItem[]>([])
  const [prescriptionsLoading, setPrescriptionsLoading] = useState(false)
  const [selectedPrescriptionId, setSelectedPrescriptionId] = useState<number | null>(null)

  // Fetch provinces
  useEffect(() => {
    let cancelled = false
    fetch('https://provinces.open-api.vn/api/v2/p/')
      .then((res) => res.json())
      .then((data: VnProvince[]) => {
        if (!cancelled) setProvinces(data)
      })
      .catch(() => console.error('Failed to fetch provinces'))
      .finally(() => {
        if (!cancelled) setProvincesLoading(false)
      })
    return () => {
      cancelled = true
    }
  }, [])

  // Fetch wards when province changes
  useEffect(() => {
    const prov = newAddress.province
      ? provinces.find((p) => p.name === newAddress.province)
      : null
    let cancelled = false
    if (!prov) {
      // Province bi clear -> reset wards (lich su) trong .then de tranh sync setState
      Promise.resolve().then(() => {
        if (!cancelled) setWards([])
      })
      return () => {
        cancelled = true
      }
    }
    Promise.resolve()
      .then(() => {
        if (!cancelled) setWardsLoading(true)
      })
      .then(() => fetch(`https://provinces.open-api.vn/api/v2/p/${prov.code}?depth=2`))
      .then((res) => res.json())
      .then((data: { wards?: VnWard[] }) => {
        if (!cancelled) setWards(data.wards ?? [])
      })
      .catch(() => console.error('Failed to fetch wards'))
      .finally(() => {
        if (!cancelled) setWardsLoading(false)
      })
    return () => {
      cancelled = true
    }
  }, [newAddress.province, provinces])

  // Load addresses lan dau
  useEffect(() => {
    let cancelled = false
    addressesApi
      .listMy()
      .then((res) => {
        if (cancelled) return
        const list = res.items.filter((a) => a.isActive)
        setAddresses(list)
        // Auto-select default neu co, neu khong lay dau tien
        const def = list.find((a) => a.isDefault) ?? list[0]
        if (def) {
          setSelectedAddressId(def.id)
          setShowNewAddressForm(false)
        } else {
          // Khong co address -> tu dong show form
          setShowNewAddressForm(true)
        }
      })
      .catch((err) => {
        if (cancelled) return
        setSubmitError(extractApiError(err, 'Không tải được danh sách địa chỉ'))
        setShowNewAddressForm(true)
      })
      .finally(() => {
        if (!cancelled) setAddressesLoading(false)
      })
    return () => {
      cancelled = true
    }
  }, [])

  // Load prescriptions if cart has required meds
  useEffect(() => {
    if (!cart?.hasPrescriptionRequired) return

    let cancelled = false
    // eslint-disable-next-line react-hooks/set-state-in-effect -- fetch-then-setState voi cancelled flag chong race
    setPrescriptionsLoading(true)

    prescriptionsApi
      .listMy({ page: 1, pageSize: 50 })
      .then((res) => {
        if (cancelled) return

        const usable = res.items.filter(
          (p) =>
            p.verificationStatus === 'verified' &&
            (p.status === 'draft' || p.status === 'active'),
        )

        setVerifiedPrescriptions(usable)

        if (usable.length > 0) {
          setSelectedPrescriptionId((current) => current ?? usable[0].id)
        }
      })
      .catch((err) => {
        if (!cancelled) {
          setSubmitError(extractApiError(err, 'Không tải được danh sách đơn thuốc'))
        }
      })
      .finally(() => {
        if (!cancelled) setPrescriptionsLoading(false)
      })

    return () => {
      cancelled = true
    }
  }, [cart?.hasPrescriptionRequired])

  const shippingFee = deliveryMethod === 'standard' ? SHIPPING_FEE : EXPRESS_SHIPPING_FEE
  const orderTotal = cart ? cart.total + shippingFee : 0

  if (!cart || items.length === 0) {
    return (
      <div className="pt-12 pb-24 px-4 md:px-12 max-w-7xl mx-auto flex flex-col items-center justify-center min-h-[50vh]">
        <span className="material-symbols-outlined text-6xl text-outline mb-4">shopping_basket</span>
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
    )
  }

  const updateField = (key: keyof NewAddressForm, value: string) => {
    setNewAddress((p) => ({ ...p, [key]: value }))
    if (fieldErrors[key]) {
      setFieldErrors((p) => ({ ...p, [key]: undefined }))
    }
  }

  const handleSubmit = async () => {
    setSubmitError(null)

    let addressId: number | null = selectedAddressId

    // Neu user dang dien form -> validate + tao address moi truoc
    if (showNewAddressForm) {
      const errors = validateAddressForm(newAddress)
      if (Object.keys(errors).length > 0) {
        setFieldErrors(errors)
        return
      }
      try {
        setSubmitting(true)
        const created = await addressesApi.create({
          recipientName: newAddress.recipientName.trim(),
          phone: newAddress.phone.replace(/\s/g, ''),
          province: newAddress.province.trim(),
          district: '', // Set empty district for backward compatibility
          ward: newAddress.ward.trim(),
          streetAddress: newAddress.streetAddress.trim(),
          isDefault: addresses.length === 0, // address dau tien -> auto default
        })
        addressId = created.id
        setAddresses((p) => [...p, created])
      } catch (err) {
        setSubmitting(false)
        setSubmitError(extractApiError(err, 'Không thể lưu địa chỉ'))
        return
      }
    }

    if (!addressId) {
      setSubmitError('Vui lòng chọn hoặc thêm địa chỉ giao hàng')
      setSubmitting(false)
      return
    }

    if (cart.hasPrescriptionRequired && !selectedPrescriptionId) {
      setSubmitError('Giỏ hàng có thuốc kê đơn. Vui lòng chọn đơn thuốc đã được dược sĩ xác minh.')
      setSubmitting(false)
      return
    }

    try {
      setSubmitting(true)
      const order = await ordersApi.checkout({
        addressId,
        paymentMethodId: null,
        paymentType,
        prescriptionId: cart.hasPrescriptionRequired ? selectedPrescriptionId : null,
      })
      // Refresh cart de UI badge ve 0
      await dispatch(fetchCartThunk())
      navigate(`/orders/${order.id}`)
    } catch (err) {
      setSubmitting(false)
      setSubmitError(extractApiError(err, 'Đặt hàng thất bại'))
    }
  }

  return (
    <div className="pt-12 pb-24 px-4 md:px-12 max-w-7xl mx-auto animate-in fade-in zoom-in-95 duration-500">
      <header className="mb-12">
        <h1 className="text-4xl md:text-5xl font-extrabold tracking-tight text-on-surface mb-2 font-headline">
          Thanh toán
        </h1>
        <p className="text-on-surface-variant font-medium">
          Hoàn tất đơn hàng của bạn với bảo mật cấp độ y tế.
        </p>
      </header>

      <div className="flex flex-col lg:flex-row gap-12">
        {/* Left Side: Forms */}
        <div className="flex-1 space-y-12">
          {/* Shipping Info Section */}
          <section>
            <div className="flex items-center justify-between mb-6">
              <div className="flex items-center gap-3">
                <span
                  className="material-symbols-outlined text-primary"
                  style={{ fontVariationSettings: "'FILL' 1" }}
                >
                  local_shipping
                </span>
                <h2 className="text-2xl font-bold tracking-tight font-headline">
                  Thông tin giao hàng
                </h2>
              </div>
              {addresses.length > 0 && !showNewAddressForm && (
                <button
                  type="button"
                  onClick={() => {
                    setShowNewAddressForm(true)
                    setNewAddress(EMPTY_ADDRESS)
                    setFieldErrors({})
                  }}
                  className="text-sm font-semibold text-primary hover:underline"
                >
                  + Thêm địa chỉ mới
                </button>
              )}
            </div>

            {/* Saved address list */}
            {!showNewAddressForm && (
              <div className="space-y-3">
                {addressesLoading && (
                  <div className="bg-surface-container-low p-6 rounded-xl text-on-surface-variant">
                    Đang tải địa chỉ...
                  </div>
                )}
                {!addressesLoading &&
                  addresses.map((addr) => {
                    const selected = addr.id === selectedAddressId
                    return (
                      <label
                        key={addr.id}
                        className={`relative flex items-start gap-4 p-5 border-2 rounded-xl cursor-pointer transition-all ${
                          selected
                            ? 'bg-surface-container-lowest border-primary'
                            : 'bg-surface-container-low border-transparent hover:border-outline-variant'
                        }`}
                        onClick={() => setSelectedAddressId(addr.id)}
                      >
                        <input
                          className="hidden"
                          type="radio"
                          name="address"
                          checked={selected}
                          readOnly
                        />
                        <div className="flex-1 min-w-0">
                          <div className="flex items-center gap-2 mb-1">
                            <span className="font-bold text-base">{addr.recipientName}</span>
                            <span className="text-sm text-on-surface-variant">{addr.phone}</span>
                            {addr.isDefault && (
                              <span className="text-xs px-2 py-0.5 rounded-full bg-primary-fixed text-on-primary-fixed font-semibold">
                                Mặc định
                              </span>
                            )}
                          </div>
                          <div className="text-sm text-on-surface-variant">{addr.fullAddress}</div>
                        </div>
                        {selected && (
                          <span
                            className="material-symbols-outlined text-primary"
                            style={{ fontVariationSettings: "'FILL' 1" }}
                          >
                            check_circle
                          </span>
                        )}
                      </label>
                    )
                  })}
              </div>
            )}

            {/* New address form */}
            {showNewAddressForm && (
              <div className="bg-surface-container-low p-8 rounded-xl space-y-6">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  <div className="space-y-2">
                    <label className="text-sm font-semibold text-on-surface-variant px-1">
                      Họ và tên
                    </label>
                    <input
                      className="w-full h-12 px-4 rounded-lg bg-surface-container-lowest border-none focus:ring-2 focus:ring-primary transition-all outline-none"
                      placeholder="Nguyễn Văn A"
                      type="text"
                      value={newAddress.recipientName}
                      onChange={(e) => updateField('recipientName', e.target.value)}
                    />
                    {fieldErrors.recipientName && (
                      <p className="text-xs text-error font-medium px-1">
                        {fieldErrors.recipientName}
                      </p>
                    )}
                  </div>
                  <div className="space-y-2">
                    <label className="text-sm font-semibold text-on-surface-variant px-1">
                      Số điện thoại
                    </label>
                    <input
                      className="w-full h-12 px-4 rounded-lg bg-surface-container-lowest border-none focus:ring-2 focus:ring-primary transition-all outline-none"
                      placeholder="0901234567"
                      type="tel"
                      value={newAddress.phone}
                      onChange={(e) => updateField('phone', e.target.value)}
                    />
                    {fieldErrors.phone && (
                      <p className="text-xs text-error font-medium px-1">{fieldErrors.phone}</p>
                    )}
                  </div>
                  <div className="md:col-span-2 space-y-2">
                    <label className="text-sm font-semibold text-on-surface-variant px-1">
                      Số nhà, tên đường
                    </label>
                    <input
                      className="w-full h-12 px-4 rounded-lg bg-surface-container-lowest border-none focus:ring-2 focus:ring-primary transition-all outline-none"
                      placeholder="123 Nguyễn Trãi"
                      type="text"
                      value={newAddress.streetAddress}
                      onChange={(e) => updateField('streetAddress', e.target.value)}
                    />
                    {fieldErrors.streetAddress && (
                      <p className="text-xs text-error font-medium px-1">
                        {fieldErrors.streetAddress}
                      </p>
                    )}
                  </div>
                  <div className="space-y-2">
                    <label className="text-sm font-semibold text-on-surface-variant px-1">
                      Tỉnh/Thành phố
                    </label>
                    <SearchableDropdown
                      placeholder="Chọn tỉnh/thành phố"
                      value={newAddress.province}
                      disabled={provincesLoading}
                      options={provinces.map((p) => ({ label: p.name, value: p.name }))}
                      onChange={(val) => {
                        updateField('province', val)
                        updateField('ward', '') // Reset ward when province changes
                      }}
                    />
                    {fieldErrors.province && (
                      <p className="text-xs text-error font-medium px-1">{fieldErrors.province}</p>
                    )}
                  </div>
                  <div className="space-y-2">
                    <label className="text-sm font-semibold text-on-surface-variant px-1">
                      Phường/Xã
                    </label>
                    <SearchableDropdown
                      placeholder="Chọn phường/xã"
                      value={newAddress.ward}
                      disabled={!newAddress.province || wardsLoading}
                      options={wards.map((w) => ({ label: w.name, value: w.name }))}
                      onChange={(val) => updateField('ward', val)}
                    />
                    {fieldErrors.ward && (
                      <p className="text-xs text-error font-medium px-1">{fieldErrors.ward}</p>
                    )}
                  </div>
                </div>
                {addresses.length > 0 && (
                  <button
                    type="button"
                    onClick={() => {
                      setShowNewAddressForm(false)
                      setFieldErrors({})
                    }}
                    className="text-sm font-semibold text-on-surface-variant hover:underline"
                  >
                    ← Dùng địa chỉ đã lưu
                  </button>
                )}
              </div>
            )}
          </section>

          {/* Delivery Method Section */}
          <section>
            <div className="flex items-center gap-3 mb-6">
              <span
                className="material-symbols-outlined text-primary"
                style={{ fontVariationSettings: "'FILL' 1" }}
              >
                package_2
              </span>
              <h2 className="text-2xl font-bold tracking-tight font-headline">
                Phương thức vận chuyển
              </h2>
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
                    <span
                      className="material-symbols-outlined"
                      style={{ fontVariationSettings: "'FILL' 1" }}
                    >
                      check_circle
                    </span>
                  </div>
                )}
              </label>

              <div
                className="relative flex items-center p-6 border-2 rounded-xl cursor-not-allowed opacity-60 bg-surface-container-low border-transparent"
                title="Tính năng sắp ra mắt"
              >
                <div className="flex-1">
                  <div className="font-bold text-lg">Giao hàng hỏa tốc</div>
                  <div className="text-on-surface-variant text-sm">Dự kiến nhận hàng: 24h</div>
                </div>
                <div className="font-bold text-on-surface">{formatVnd(EXPRESS_SHIPPING_FEE)}</div>
                <div className="absolute top-3 right-3 text-xs px-2 py-1 rounded-full bg-secondary-container text-on-secondary-container font-semibold">
                  Sắp ra mắt
                </div>
              </div>
            </div>
          </section>

          {/* Prescriptions Section */}
          {cart.hasPrescriptionRequired && (
            <section>
              <div className="flex items-center gap-3 mb-6">
                <span
                  className="material-symbols-outlined text-primary"
                  style={{ fontVariationSettings: "'FILL' 1" }}
                >
                  clinical_notes
                </span>
                <h2 className="text-2xl font-bold tracking-tight font-headline">
                  Đơn thuốc xác minh
                </h2>
              </div>

              <PrescriptionPicker
                prescriptions={verifiedPrescriptions}
                selectedId={selectedPrescriptionId}
                onSelect={setSelectedPrescriptionId}
                loading={prescriptionsLoading}
              />

              <div className="mt-4 flex flex-wrap gap-3">
                <button
                  type="button"
                  onClick={() => navigate('/prescriptions')}
                  className="px-4 py-2 rounded-xl bg-primary text-on-primary text-sm font-semibold hover:bg-primary/90 transition-colors"
                >
                  Quản lý đơn thuốc
                </button>
                <button
                  type="button"
                  onClick={() => navigate('/prescriptions')}
                  className="px-4 py-2 rounded-xl border border-outline-variant/40 text-sm font-semibold hover:bg-surface-container-high transition-colors"
                >
                  Upload đơn thuốc mới
                </button>
              </div>
            </section>
          )}

          {/* Payment Method Section - COD hoac chuyen khoan VietQR */}
          <section>
            <div className="flex items-center gap-3 mb-6">
              <span
                className="material-symbols-outlined text-primary"
                style={{ fontVariationSettings: "'FILL' 1" }}
              >
                account_balance_wallet
              </span>
              <h2 className="text-2xl font-bold tracking-tight font-headline">
                Phương thức thanh toán
              </h2>
            </div>
            <div className="space-y-3">
              <label
                className={`p-5 rounded-xl border-2 flex items-center gap-4 cursor-pointer transition-all ${
                  paymentType === 'cod'
                    ? 'border-primary bg-surface-container-lowest'
                    : 'border-transparent bg-surface-container-low hover:border-outline-variant'
                }`}
                onClick={() => setPaymentType('cod')}
              >
                <input type="radio" className="hidden" name="payment" checked={paymentType === 'cod'} readOnly />
                <div className="w-12 h-12 rounded-lg bg-primary-fixed flex items-center justify-center">
                  <span className="material-symbols-outlined text-on-primary-fixed">payments</span>
                </div>
                <div className="flex-1">
                  <div className="font-bold text-lg">Thanh toán khi nhận hàng (COD)</div>
                  <div className="text-sm text-on-surface-variant">
                    Trả tiền mặt cho shipper khi nhận thuốc.
                  </div>
                </div>
                {paymentType === 'cod' && (
                  <span
                    className="material-symbols-outlined text-primary"
                    style={{ fontVariationSettings: "'FILL' 1" }}
                  >
                    check_circle
                  </span>
                )}
              </label>

              <label
                className={`p-5 rounded-xl border-2 flex items-center gap-4 cursor-pointer transition-all ${
                  paymentType === 'bank_transfer'
                    ? 'border-primary bg-surface-container-lowest'
                    : 'border-transparent bg-surface-container-low hover:border-outline-variant'
                }`}
                onClick={() => setPaymentType('bank_transfer')}
              >
                <input
                  type="radio"
                  className="hidden"
                  name="payment"
                  checked={paymentType === 'bank_transfer'}
                  readOnly
                />
                <div className="w-12 h-12 rounded-lg bg-secondary-container flex items-center justify-center">
                  <span className="material-symbols-outlined text-on-secondary-container">qr_code_2</span>
                </div>
                <div className="flex-1">
                  <div className="font-bold text-lg">Chuyển khoản ngân hàng (VietQR)</div>
                  <div className="text-sm text-on-surface-variant">
                    Quét mã QR sau khi đặt — app ngân hàng tự điền số tiền và nội dung.
                  </div>
                </div>
                {paymentType === 'bank_transfer' && (
                  <span
                    className="material-symbols-outlined text-primary"
                    style={{ fontVariationSettings: "'FILL' 1" }}
                  >
                    check_circle
                  </span>
                )}
              </label>
            </div>
            <p className="mt-3 text-xs text-on-surface-variant">
              Visa/Mastercard, MoMo, ZaloPay sẽ ra mắt sau.
            </p>
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
                    <h4 className="font-bold text-sm leading-tight mb-1 truncate" title={item.name}>
                      {item.name}
                    </h4>
                    <div className="text-xs text-on-surface-variant mb-2">
                      Số lượng: {item.quantity}
                    </div>
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
              {cart.totalDiscount > 0 && (
                <div className="flex justify-between text-on-surface-variant">
                  <span>Giảm giá</span>
                  <span className="text-error">-{formatVnd(cart.totalDiscount)}</span>
                </div>
              )}

              <div className="flex justify-between items-end pt-4 border-t border-outline-variant/10">
                <span className="font-bold text-lg">Tổng cộng</span>
                <div className="text-3xl font-extrabold text-primary tracking-tighter">
                  {formatVnd(orderTotal)}
                </div>
              </div>
            </div>

            {submitError && (
              <div className="mb-4 p-3 rounded-lg bg-error-container text-on-error-container text-sm font-medium">
                {submitError}
              </div>
            )}

            <button
              className="w-full py-5 bg-gradient-to-r from-primary to-primary-container text-on-primary font-extrabold text-xl rounded-full shadow-lg shadow-primary/20 hover:scale-[1.02] active:scale-95 transition-all flex items-center justify-center gap-3 disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:scale-100"
              disabled={cart.hasUnavailableItems || submitting || addressesLoading}
              onClick={handleSubmit}
            >
              {submitting ? (
                <>
                  <span className="material-symbols-outlined animate-spin">progress_activity</span>
                  Đang xử lý...
                </>
              ) : (
                <>
                  Thanh toán ngay
                  <span className="material-symbols-outlined">arrow_forward</span>
                </>
              )}
            </button>

            {cart.hasPrescriptionRequired && !selectedPrescriptionId && (
              <p className="mt-3 text-xs text-error font-semibold text-center">
                Vui lòng chọn đơn thuốc đã xác minh để tiếp tục thanh toán.
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
  )
}
