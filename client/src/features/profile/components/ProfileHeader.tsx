// =============================================================================
// ProfileHeader - hien thi avatar + ten user va modal cap nhat ho so.
// Modal goi:
//   - usersApi.uploadAvatar (multipart) khi user chon file anh
//   - usersApi.updateProfile (PUT /users/me) cho fullName/phone/dob/avatar
//   - usersApi.changePassword (PUT /users/me/change-password) neu user nhap pass moi
// Sau khi luu, dispatch userUpdated() de Redux state phan anh ngay tren header.
// =============================================================================
import { useEffect, useRef, useState } from 'react';
import { useAuth } from '@/hooks/useAuth';
import { useAppDispatch } from '@/hooks/redux';
import { userUpdated } from '@/features/auth/auth-slice';
import { usersApi } from '@/features/users/users-api';

function toDateInput(value: string | null | undefined): string {
  if (!value) return '';
  // BE serialize DateOnly => "yyyy-MM-dd" san; phong truong hop ISO full datetime.
  return value.length >= 10 ? value.substring(0, 10) : value;
}

export function ProfileHeader() {
  const { user } = useAuth();
  const dispatch = useAppDispatch();
  const userName = user?.fullName || 'Người Dùng';

  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [formData, setFormData] = useState({
    phoneNumber: '',
    dob: '',
    avatarUrl: '',
    oldPassword: '',
    newPassword: '',
  });
  const [saving, setSaving] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement | null>(null);

  // Reset form moi lan mo modal de phan anh user moi nhat.
  useEffect(() => {
    if (isEditModalOpen) {
      setFormData({
        phoneNumber: user?.phoneNumber ?? '',
        dob: toDateInput(user?.dateOfBirth),
        avatarUrl: user?.avatarUrl ?? '',
        oldPassword: '',
        newPassword: '',
      });
      setError(null);
    }
  }, [isEditModalOpen, user]);

  const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    setUploading(true);
    setError(null);
    try {
      const url = await usersApi.uploadAvatar(file);
      setFormData((prev) => ({ ...prev, avatarUrl: url }));
    } catch (err: unknown) {
      const ex = err as { response?: { data?: { detail?: string } }; message?: string };
      setError(ex.response?.data?.detail ?? ex.message ?? 'Tải ảnh thất bại');
    } finally {
      setUploading(false);
      if (fileInputRef.current) fileInputRef.current.value = '';
    }
  };

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!user) return;

    if (formData.newPassword && !formData.oldPassword) {
      setError('Vui lòng nhập mật khẩu hiện tại để đổi mật khẩu mới.');
      return;
    }

    setSaving(true);
    setError(null);
    try {
      const updated = await usersApi.updateProfile({
        fullName: user.fullName,
        avatarUrl: formData.avatarUrl.trim() || null,
        phoneNumber: formData.phoneNumber.trim() || null,
        dateOfBirth: formData.dob || null,
      });

      if (formData.newPassword) {
        await usersApi.changePassword({
          currentPassword: formData.oldPassword,
          newPassword: formData.newPassword,
          confirmPassword: formData.newPassword,
        });
      }

      dispatch(
        userUpdated({
          avatarUrl: updated.avatarUrl,
          phoneNumber: updated.phoneNumber,
          dateOfBirth: updated.dateOfBirth,
          fullName: updated.fullName,
        }),
      );
      setIsEditModalOpen(false);
    } catch (err: unknown) {
      const ex = err as { response?: { data?: { detail?: string; errors?: Record<string, string[]> } }; message?: string };
      const errors = ex.response?.data?.errors;
      const firstFieldErr = errors ? Object.values(errors)[0]?.[0] : undefined;
      setError(firstFieldErr ?? ex.response?.data?.detail ?? ex.message ?? 'Cập nhật thất bại');
    } finally {
      setSaving(false);
    }
  };

  return (
    <header className="mb-12">
      <div className="flex flex-col md:flex-row md:items-end justify-between gap-6">
        <div className="flex items-center gap-4 relative group">
          {user?.avatarUrl ? (
            <img src={user.avatarUrl} alt="Avatar" className="w-20 h-20 md:w-24 md:h-24 rounded-full object-cover shadow-md border-4 border-surface" />
          ) : (
            <div className="w-20 h-20 md:w-24 md:h-24 bg-gradient-to-br from-primary to-primary-container text-on-primary rounded-full flex items-center justify-center text-3xl font-bold shadow-md border-4 border-surface">
              {userName.charAt(0).toUpperCase()}
            </div>
          )}

          <button
            onClick={() => setIsEditModalOpen(true)}
            className="absolute bottom-0 left-16 md:left-20 bg-surface border border-outline-variant text-on-surface-variant hover:text-primary rounded-full p-1.5 shadow-sm transition-colors"
            title="Chỉnh sửa ảnh đại diện"
          >
            <span className="material-symbols-outlined text-[16px]">edit</span>
          </button>

          <div className="ml-2">
            <span className="text-primary font-semibold tracking-wider text-sm uppercase">Chào mừng trở lại,</span>
            <div className="flex items-center gap-3 mt-1">
              <h1 className="text-4xl md:text-5xl font-extrabold tracking-tighter text-on-surface">{userName}</h1>
              <button
                onClick={() => setIsEditModalOpen(true)}
                className="text-on-surface-variant hover:text-primary transition-colors mt-2"
                title="Chỉnh sửa thông tin"
              >
                <span className="material-symbols-outlined text-[24px]">edit_note</span>
              </button>
            </div>
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

      {/* Edit Profile Modal */}
      {isEditModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/40 backdrop-blur-sm animate-in fade-in duration-200">
          <div className="bg-surface text-on-surface w-full max-w-lg rounded-3xl shadow-2xl overflow-hidden border border-outline-variant/30 flex flex-col max-h-[90vh]">
            <div className="flex items-center justify-between px-6 py-4 border-b border-outline-variant/30">
              <h2 className="text-xl font-bold font-headline flex items-center gap-2">
                <span className="material-symbols-outlined text-primary">manage_accounts</span>
                Cập nhật hồ sơ cá nhân
              </h2>
              <button
                onClick={() => setIsEditModalOpen(false)}
                className="p-2 text-on-surface-variant hover:bg-surface-container rounded-full transition-colors"
              >
                <span className="material-symbols-outlined text-[24px]">close</span>
              </button>
            </div>

            <div className="p-6 overflow-y-auto">
              {error && (
                <div className="mb-4 px-4 py-3 rounded-xl bg-error-container/30 border border-error/30 text-error text-sm">
                  {error}
                </div>
              )}
              <form id="edit-profile-form" onSubmit={handleSave} className="space-y-5">
                {/* Personal Info Section */}
                <div className="space-y-4">
                  <h3 className="text-sm font-bold text-primary uppercase tracking-wider">Thông tin liên hệ</h3>

                  <div>
                    <label className="block text-sm font-semibold mb-1.5 text-on-surface">Ảnh đại diện</label>
                    <div className="flex items-center gap-4">
                      {formData.avatarUrl ? (
                        <img src={formData.avatarUrl} alt="preview" className="w-16 h-16 rounded-full object-cover border border-outline-variant" />
                      ) : (
                        <div className="w-16 h-16 rounded-full bg-surface-container-high flex items-center justify-center text-on-surface-variant">
                          <span className="material-symbols-outlined">person</span>
                        </div>
                      )}
                      <div className="flex-1 flex items-center gap-2">
                        <input
                          ref={fileInputRef}
                          type="file"
                          accept="image/*"
                          onChange={handleFileChange}
                          disabled={uploading}
                          className="text-sm text-on-surface-variant file:mr-3 file:px-3 file:py-2 file:rounded-lg file:border-0 file:bg-primary/10 file:text-primary file:font-semibold hover:file:bg-primary/20 file:cursor-pointer"
                        />
                        {formData.avatarUrl && (
                          <button
                            type="button"
                            onClick={() => setFormData({ ...formData, avatarUrl: '' })}
                            className="p-1.5 text-on-surface-variant hover:text-error rounded-lg transition-colors"
                            title="Bỏ ảnh đại diện"
                          >
                            <span className="material-symbols-outlined text-[20px]">close</span>
                          </button>
                        )}
                      </div>
                    </div>
                    {uploading && <p className="mt-2 text-xs text-primary">Đang tải ảnh lên...</p>}
                    <p className="mt-1 text-xs text-on-surface-variant">JPG/PNG/WebP/GIF, tối đa 5 MB.</p>
                  </div>

                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-semibold mb-1.5 text-on-surface">Số điện thoại</label>
                      <input
                        type="tel"
                        value={formData.phoneNumber}
                        onChange={(e) => setFormData({ ...formData, phoneNumber: e.target.value })}
                        placeholder="0912345678"
                        className="w-full px-4 py-2.5 rounded-xl border border-outline-variant bg-surface focus:outline-none focus:ring-2 focus:ring-primary/50 text-sm"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-semibold mb-1.5 text-on-surface">Ngày sinh</label>
                      <input
                        type="date"
                        value={formData.dob}
                        max={new Date().toISOString().split('T')[0]}
                        onChange={(e) => setFormData({ ...formData, dob: e.target.value })}
                        className="w-full px-4 py-2.5 rounded-xl border border-outline-variant bg-surface focus:outline-none focus:ring-2 focus:ring-primary/50 text-sm"
                      />
                    </div>
                  </div>
                </div>

                <div className="border-t border-outline-variant/30 my-4"></div>

                {/* Password Section */}
                <div className="space-y-4">
                  <h3 className="text-sm font-bold text-primary uppercase tracking-wider">Bảo mật</h3>
                  <p className="text-xs text-on-surface-variant">Để trống nếu không đổi mật khẩu.</p>

                  <div>
                    <label className="block text-sm font-semibold mb-1.5 text-on-surface">Mật khẩu hiện tại</label>
                    <input
                      type="password"
                      value={formData.oldPassword}
                      onChange={(e) => setFormData({ ...formData, oldPassword: e.target.value })}
                      placeholder="Nhập để xác thực đổi mật khẩu"
                      className="w-full px-4 py-2.5 rounded-xl border border-outline-variant bg-surface focus:outline-none focus:ring-2 focus:ring-primary/50 text-sm"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-semibold mb-1.5 text-on-surface">Mật khẩu mới</label>
                    <input
                      type="password"
                      value={formData.newPassword}
                      onChange={(e) => setFormData({ ...formData, newPassword: e.target.value })}
                      placeholder="Nhập mật khẩu mới"
                      className="w-full px-4 py-2.5 rounded-xl border border-outline-variant bg-surface focus:outline-none focus:ring-2 focus:ring-primary/50 text-sm"
                    />
                  </div>
                </div>
              </form>
            </div>

            <div className="px-6 py-4 bg-surface-container/30 border-t border-outline-variant/30 flex justify-end gap-3">
              <button
                type="button"
                onClick={() => setIsEditModalOpen(false)}
                disabled={saving}
                className="px-5 py-2.5 rounded-xl font-semibold text-on-surface-variant hover:bg-surface-container-high transition-colors disabled:opacity-50"
              >
                Đóng
              </button>
              <button
                type="submit"
                form="edit-profile-form"
                disabled={saving || uploading}
                className="px-6 py-2.5 rounded-xl font-semibold bg-primary text-on-primary hover:bg-primary/90 shadow-md shadow-primary/20 transition-all disabled:opacity-60"
              >
                {saving ? 'Đang lưu...' : 'Lưu thay đổi'}
              </button>
            </div>
          </div>
        </div>
      )}
    </header>
  );
}
