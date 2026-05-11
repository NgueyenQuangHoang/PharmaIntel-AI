import { useEffect, useMemo, useState } from 'react'
import axios from 'axios'
import { medicationRemindersApi } from '@/features/medication-reminders/medication-reminders-api'
import type {
  MedicationReminderListItem,
  MedicationReminderLogStatus,
  MedicationReminderStatus,
} from '@/features/medication-reminders/types'

type Tab = 'active' | 'paused' | 'completed' | 'cancelled'

const TABS: Record<Tab, string> = {
  active: 'Đang dùng',
  paused: 'Tạm dừng',
  completed: 'Hoàn thành',
  cancelled: 'Đã hủy',
}

function extractApiError(err: unknown, fallback: string) {
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

function formatTime(time: string) {
  return time.slice(0, 5)
}

// Tao scheduledAt local theo ngay hom nay + gio reminder. Dung de log uong/bo qua.
function toLocalScheduledAtToday(reminderTime: string) {
  const [hh = '00', mm = '00', ss = '00'] = reminderTime.split(':')
  const now = new Date()
  const yyyy = now.getFullYear()
  const month = String(now.getMonth() + 1).padStart(2, '0')
  const day = String(now.getDate()).padStart(2, '0')
  return `${yyyy}-${month}-${day}T${hh.padStart(2, '0')}:${mm.padStart(2, '0')}:${ss.padStart(2, '0')}`
}

function toUpdateBody(reminder: MedicationReminderListItem, status: MedicationReminderStatus) {
  // Sanitize: data cu co the co prescriptionItemId = 0 (khong hop le voi validator
  // GreaterThan(0)). Chi gui khi co id > 0; con lai bo qua de backend hieu la null.
  const prescriptionItemId =
    reminder.prescriptionItemId && reminder.prescriptionItemId > 0
      ? reminder.prescriptionItemId
      : null
  // Tuong tu cho startDate = "0001-01-01" (DateOnly default cho row cu) - bo qua
  // de service giu lai startDate hien tai trong DB.
  const isPlaceholderDate = (d: string | null) => !d || d.startsWith('0001-')
  return {
    prescriptionItemId,
    medicationName: reminder.medicationName,
    frequencyType: reminder.frequencyType,
    reminderTime: reminder.reminderTime,
    startDate: isPlaceholderDate(reminder.startDate) ? undefined : reminder.startDate,
    endDate: reminder.endDate,
    status,
  }
}

export function MedicationRemindersPage() {
  const [tab, setTab] = useState<Tab>('active')
  const [q, setQ] = useState('')
  const [items, setItems] = useState<MedicationReminderListItem[]>([])
  const [loading, setLoading] = useState(true)
  const [busyId, setBusyId] = useState<number | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [markedToday, setMarkedToday] = useState<Record<number, MedicationReminderLogStatus>>({})

  async function load() {
    setLoading(true)
    setError(null)
    try {
      const res = await medicationRemindersApi.list({
        status: tab,
        q: q.trim() || undefined,
        pageSize: 50,
      })
      setItems(res.items)
    } catch (err) {
      setError(extractApiError(err, 'Không tải được lịch nhắc thuốc'))
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    load()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [tab])

  const sortedItems = useMemo(
    () => [...items].sort((a, b) => a.reminderTime.localeCompare(b.reminderTime)),
    [items],
  )

  async function mark(reminder: MedicationReminderListItem, status: MedicationReminderLogStatus) {
    setBusyId(reminder.id)
    setError(null)
    try {
      await medicationRemindersApi.addLog(reminder.id, {
        scheduledAt: toLocalScheduledAtToday(reminder.reminderTime),
        status,
      })
      setMarkedToday((prev) => ({ ...prev, [reminder.id]: status }))
    } catch (err) {
      setError(extractApiError(err, 'Không cập nhật trạng thái uống thuốc'))
    } finally {
      setBusyId(null)
    }
  }

  async function changeStatus(reminder: MedicationReminderListItem, status: MedicationReminderStatus) {
    setBusyId(reminder.id)
    setError(null)
    try {
      await medicationRemindersApi.update(reminder.id, toUpdateBody(reminder, status))
      await load()
    } catch (err) {
      setError(extractApiError(err, 'Không cập nhật lịch nhắc'))
    } finally {
      setBusyId(null)
    }
  }

  return (
    <div className="pt-8 pb-24 px-6 md:px-8 max-w-6xl mx-auto animate-in fade-in zoom-in-95 duration-500">
      <header className="mb-6">
        <button
          type="button"
          onClick={() => history.back()}
          className="mb-4 flex items-center gap-1 text-sm font-semibold text-on-surface-variant hover:text-primary"
        >
          <span className="material-symbols-outlined text-[18px]">arrow_back</span>
          Quay lại
        </button>
        <h1 className="text-4xl font-extrabold tracking-tight mb-2">Lịch nhắc uống thuốc</h1>
        <p className="text-on-surface-variant font-medium">
          Theo dõi các lịch uống thuốc được tạo từ đơn đã xác minh hoặc lịch bạn tự thêm.
        </p>
      </header>

      <div className="mb-5 flex flex-col md:flex-row gap-3 md:items-center md:justify-between">
        <div className="flex gap-2 overflow-x-auto">
          {(Object.keys(TABS) as Tab[]).map((t) => (
            <button
              key={t}
              type="button"
              onClick={() => setTab(t)}
              className={`px-4 py-2 rounded-full text-sm font-bold transition-colors ${
                tab === t
                  ? 'bg-primary text-on-primary'
                  : 'bg-surface-container-low text-on-surface-variant hover:text-on-surface'
              }`}
            >
              {TABS[t]}
            </button>
          ))}
        </div>

        <div className="flex gap-2">
          <input
            value={q}
            onChange={(e) => setQ(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === 'Enter') load()
            }}
            placeholder="Tìm tên thuốc..."
            className="w-full md:w-72 rounded-xl bg-surface px-4 py-2 text-sm border border-outline-variant/30 focus:outline-none focus:ring-2 focus:ring-primary"
          />
          <button
            type="button"
            onClick={load}
            className="px-4 py-2 rounded-xl bg-primary text-on-primary font-bold text-sm"
          >
            Tìm
          </button>
        </div>
      </div>

      {error && (
        <div className="mb-4 p-3 rounded-xl bg-error-container text-on-error-container text-sm font-medium">
          {error}
        </div>
      )}

      {loading && (
        <div className="p-8 rounded-2xl bg-surface-container-low text-on-surface-variant">
          Đang tải lịch nhắc...
        </div>
      )}

      {!loading && sortedItems.length === 0 && (
        <div className="p-10 rounded-2xl bg-surface-container-low text-center border border-outline-variant/20">
          <span className="material-symbols-outlined text-5xl text-outline mb-3">alarm_off</span>
          <h2 className="text-xl font-bold">Không có lịch nhắc</h2>
          <p className="text-on-surface-variant mt-1">
            Khi dược sĩ xác minh đơn thuốc, lịch uống thuốc sẽ xuất hiện ở đây.
          </p>
        </div>
      )}

      <div className="space-y-3">
        {sortedItems.map((reminder) => {
          const marked = markedToday[reminder.id]
          const isPrescriptionBased = reminder.prescriptionItemId !== null

          return (
            <article
              key={reminder.id}
              className="rounded-2xl bg-surface-container-lowest border border-outline-variant/10 p-5 shadow-sm flex flex-col md:flex-row md:items-center gap-4"
            >
              <div className="flex items-center gap-4 flex-1">
                <div className="w-16 h-16 rounded-2xl bg-primary-container text-on-primary-container flex items-center justify-center font-extrabold text-lg">
                  {formatTime(reminder.reminderTime)}
                </div>
                <div>
                  <h2 className="font-bold text-lg">{reminder.medicationName}</h2>
                  <p className="text-sm text-on-surface-variant">
                    {reminder.frequencyType}
                    {' • '}
                    từ {new Date(reminder.startDate).toLocaleDateString('vi-VN')}
                    {reminder.endDate
                      ? ` đến ${new Date(reminder.endDate).toLocaleDateString('vi-VN')}`
                      : ' • chưa có ngày kết thúc'}
                  </p>
                  <p className="text-xs mt-1 text-outline">
                    {isPrescriptionBased ? 'Tạo từ đơn thuốc đã xác minh' : 'Lịch nhắc tự tạo'}
                  </p>
                </div>
              </div>

              <div className="flex flex-wrap gap-2 justify-end">
                {tab === 'active' && (
                  <>
                    <button
                      type="button"
                      disabled={busyId === reminder.id || marked === 'taken'}
                      onClick={() => mark(reminder, 'taken')}
                      className="px-4 py-2 rounded-xl bg-green-100 text-green-800 font-bold text-sm disabled:opacity-50"
                    >
                      {marked === 'taken' ? 'Đã ghi nhận' : 'Đã uống'}
                    </button>
                    <button
                      type="button"
                      disabled={busyId === reminder.id || marked === 'skipped'}
                      onClick={() => mark(reminder, 'skipped')}
                      className="px-4 py-2 rounded-xl bg-surface-container-high text-on-surface font-bold text-sm disabled:opacity-50"
                    >
                      Bỏ qua
                    </button>
                    <button
                      type="button"
                      disabled={busyId === reminder.id}
                      onClick={() => changeStatus(reminder, 'paused')}
                      className="px-4 py-2 rounded-xl bg-secondary-container text-on-secondary-container font-bold text-sm disabled:opacity-50"
                    >
                      Tạm dừng
                    </button>
                    <button
                      type="button"
                      disabled={busyId === reminder.id}
                      onClick={() => changeStatus(reminder, 'cancelled')}
                      className="px-4 py-2 rounded-xl bg-error-container text-on-error-container font-bold text-sm disabled:opacity-50"
                    >
                      Hủy
                    </button>
                  </>
                )}

                {tab === 'paused' && (
                  <button
                    type="button"
                    disabled={busyId === reminder.id}
                    onClick={() => changeStatus(reminder, 'active')}
                    className="px-4 py-2 rounded-xl bg-primary text-on-primary font-bold text-sm disabled:opacity-50"
                  >
                    Tiếp tục
                  </button>
                )}
              </div>
            </article>
          )
        })}
      </div>
    </div>
  )
}
