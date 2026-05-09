// =============================================================================
// BankTransferQr - Hien thi QR VietQR + thong tin chuyen khoan cho 1 don hang.
// Backend tra ve `vietQrUrl` san (img.vietqr.io PNG) khi paymentType = bank_transfer.
// =============================================================================
import { useState } from 'react'
import { formatVnd } from '@/utils/format'

type Props = {
  qrUrl: string
  transferContent: string
  total: number
  orderCode: string
}

export function BankTransferQr({ qrUrl, transferContent, total, orderCode }: Props) {
  const [copied, setCopied] = useState<'content' | 'amount' | null>(null)

  const copy = async (text: string, kind: 'content' | 'amount') => {
    try {
      await navigator.clipboard.writeText(text)
      setCopied(kind)
      setTimeout(() => setCopied((c) => (c === kind ? null : c)), 2000)
    } catch {
      /* clipboard not available */
    }
  }

  return (
    <section className="bg-gradient-to-br from-primary-container to-secondary-container rounded-2xl p-6 md:p-8 border border-outline-variant/10 mb-8">
      <div className="flex items-center gap-3 mb-6">
        <span
          className="material-symbols-outlined text-on-primary-container"
          style={{ fontVariationSettings: "'FILL' 1" }}
        >
          qr_code_2
        </span>
        <h2 className="text-xl md:text-2xl font-bold tracking-tight text-on-primary-container font-headline">
          Quét QR để thanh toán
        </h2>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-[auto,1fr] gap-6 items-start">
        {/* QR Image */}
        <div className="bg-white rounded-2xl p-3 shadow-lg shadow-slate-900/10 mx-auto md:mx-0">
          <img
            src={qrUrl}
            alt={`VietQR thanh toán đơn ${orderCode}`}
            className="w-64 h-64 object-contain"
            loading="lazy"
          />
        </div>

        {/* Info */}
        <div className="space-y-4 text-on-primary-container">
          <p className="text-sm font-medium opacity-90">
            Mở app ngân hàng → quét QR. Số tiền và nội dung chuyển khoản đã được điền sẵn.
          </p>

          <div className="bg-white/60 dark:bg-black/20 backdrop-blur rounded-xl p-4 space-y-3">
            <div>
              <div className="text-xs font-semibold uppercase tracking-wide opacity-70 mb-1">
                Số tiền
              </div>
              <div className="flex items-center justify-between gap-2">
                <span className="text-2xl font-extrabold tracking-tight">{formatVnd(total)}</span>
                <button
                  onClick={() => copy(String(Math.round(total)), 'amount')}
                  className="text-xs px-3 py-1.5 rounded-full bg-white/80 hover:bg-white font-semibold transition-colors"
                >
                  {copied === 'amount' ? 'Đã chép!' : 'Sao chép'}
                </button>
              </div>
            </div>

            <div className="border-t border-on-primary-container/10 pt-3">
              <div className="text-xs font-semibold uppercase tracking-wide opacity-70 mb-1">
                Nội dung chuyển khoản
              </div>
              <div className="flex items-center justify-between gap-2">
                <code className="text-base font-bold font-mono break-all">{transferContent}</code>
                <button
                  onClick={() => copy(transferContent, 'content')}
                  className="text-xs px-3 py-1.5 rounded-full bg-white/80 hover:bg-white font-semibold transition-colors shrink-0"
                >
                  {copied === 'content' ? 'Đã chép!' : 'Sao chép'}
                </button>
              </div>
            </div>
          </div>

          <div className="text-xs opacity-80 leading-relaxed">
            <strong>Lưu ý:</strong> Vui lòng giữ nguyên nội dung chuyển khoản để hệ thống đối soát đơn hàng. Đơn sẽ được xác nhận sau khi nhận được thanh toán.
          </div>
        </div>
      </div>
    </section>
  )
}
