import { useAuth } from '@/hooks/useAuth';

export function ProfileHeader() {
  const { user } = useAuth();
  const userName = user?.fullName || 'Người Dùng';

  return (
    <header className="mb-12">
      <div className="flex flex-col md:flex-row md:items-end justify-between gap-6">
        <div className="flex items-center gap-4">
          {user?.avatarUrl ? (
            <img src={user.avatarUrl} alt="Avatar" className="w-16 h-16 rounded-full object-cover shadow-sm" />
          ) : (
            <div className="w-16 h-16 bg-gradient-to-br from-primary to-primary-container text-on-primary rounded-full flex items-center justify-center text-2xl font-bold shadow-sm">
              {userName.charAt(0).toUpperCase()}
            </div>
          )}
          <div>
            <span className="text-primary font-semibold tracking-wider text-sm uppercase">Chào mừng trở lại,</span>
            <h1 className="text-4xl md:text-5xl font-extrabold tracking-tighter text-on-surface mt-2">{userName}</h1>
          </div>
        </div>
        <div className="flex gap-3">
          <button className="px-6 py-3 bg-secondary-fixed text-on-secondary-fixed rounded-full font-semibold transition-opacity hover:opacity-80">
            Xuất báo cáo PDF
          </button>
          <button className="px-6 py-3 bg-gradient-to-br from-primary to-primary-container text-on-primary rounded-full font-semibold transition-opacity hover:opacity-90 flex items-center gap-2">
            <span className="material-symbols-outlined">add</span>
            Tạo chẩn đoán mới
          </button>
        </div>
      </div>
    </header>
  );
}
