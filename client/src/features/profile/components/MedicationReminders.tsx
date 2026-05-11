import { Link } from 'react-router-dom';
import { useAppSelector } from '@/hooks/redux';

export function MedicationReminders() {
  const { data, status } = useAppSelector((state) => state.profile.reminders);

  const formatTime = (timeStr: string) => {
    // timeStr from backend TimeOnly is usually "HH:mm:ss"
    return timeStr.substring(0, 5);
  };

  const formatDateOnlyShort = (date: string | null) => {
    if (!date) return '';
    const [y, m, d] = date.split('-');
    return `${d}/${m}`;
  };

  const isUpcoming = (timeStr: string) => {
    const now = new Date();
    const currentMins = now.getHours() * 60 + now.getMinutes();
    const [hours, mins] = timeStr.split(':').map(Number);
    const reminderMins = hours * 60 + mins;

    // Sắp tới nếu thời gian nhắc trong vòng 2 tiếng tới
    return reminderMins > currentMins && reminderMins <= currentMins + 120;
  };

  const isPassed = (timeStr: string) => {
    const now = new Date();
    const currentMins = now.getHours() * 60 + now.getMinutes();
    const [hours, mins] = timeStr.split(':').map(Number);
    const reminderMins = hours * 60 + mins;

    return reminderMins <= currentMins;
  };

  // Tach activeReminders truoc render de check empty/preview cung dung 1 nguon
  // (tranh case data co toan completed/cancelled nhung khong hien "Chua co" do
  // length != 0 nhung filter ra rong).
  const activeReminders = data?.items.filter((r) => r.status === 'active') ?? [];

  return (
    <aside className="md:col-span-4 bg-primary-fixed p-8 rounded-xl flex flex-col">
      <div className="flex items-center gap-3 mb-6">
        <span className="material-symbols-outlined text-primary">alarm</span>
        <h2 className="text-xl font-bold text-on-primary-fixed">Nhắc nhở uống thuốc</h2>
      </div>
      <div className="space-y-4 flex-grow">
        {status === 'loading' && !data && (
          <div className="text-center text-on-primary-fixed py-4 animate-pulse">Đang tải...</div>
        )}

        {status === 'success' && activeReminders.length === 0 && (
          <div className="text-center text-on-primary-fixed py-4">Chưa có lịch nhắc nhở nào</div>
        )}

        {activeReminders.slice(0, 3).map((reminder) => {
          const upcoming = isUpcoming(reminder.reminderTime);
          const passed = isPassed(reminder.reminderTime);

          return (
            <div
              key={reminder.id}
              className={`bg-surface-container-lowest p-4 rounded-lg flex items-center justify-between shadow-sm ${passed ? 'opacity-60' : ''}`}
            >
              <div>
                <p className={`font-bold ${passed ? 'text-on-surface' : 'text-primary'}`}>{reminder.medicationName}</p>
                <p className="text-xs text-outline">
                  {reminder.frequencyType}
                  {reminder.endDate && (
                    <>
                      {' • đến hết '}
                      {formatDateOnlyShort(reminder.endDate)}
                    </>
                  )}
                </p>
              </div>
              <div className="text-right">
                <p className={`text-sm font-bold ${passed ? 'text-on-surface-variant line-through' : ''}`}>
                  {formatTime(reminder.reminderTime)}
                </p>
                {upcoming && (
                  <span className="text-[10px] bg-secondary-container px-2 py-0.5 rounded text-on-secondary-container font-medium mt-1 inline-block">Sắp tới</span>
                )}
                {passed && (
                  <span className="text-[10px] bg-surface-container-high px-2 py-0.5 rounded text-on-surface-variant font-medium mt-1 inline-block">
                    Đã qua giờ
                  </span>
                )}
              </div>
            </div>
          );
        })}
      </div>
      <Link
        to="/medication-reminders"
        className="mt-6 w-full py-3 bg-surface-container-lowest text-primary text-sm font-bold rounded-full border border-primary/10 hover:bg-surface-container-low transition-colors shadow-sm text-center"
      >
        Quản lý lịch nhắc
      </Link>
    </aside>
  );
}
