// =============================================================================
// AdminPharmacistsPage - Quan ly Duoc si (CRUD qua API /pharmacists).
// =============================================================================
import { useEffect, useState } from 'react';
import { AdminPageShell } from '@/features/admin/components/AdminPageShell';
import {
  pharmacistsApi,
  type PharmacistUpsertRequest,
} from '@/features/pharmacists/pharmacists-api';
import type { Pharmacist } from '@/features/pharmacists/types';

const SPECIALTIES = ['Dược lâm sàng', 'Dược liệu & Cổ truyền', 'Dinh dưỡng', 'Dược lý'];

type FormState = {
  fullName: string;
  licenseNumber: string;
  avatarUrl: string;
  specialization: string;
  experienceYears: number;
  about: string;
  isOnline: boolean;
  rating: number;
  reviewsCount: number;
};

const emptyForm = (): FormState => ({
  fullName: '',
  licenseNumber: '',
  avatarUrl: `https://i.pravatar.cc/150?u=new${Math.random()}`,
  specialization: SPECIALTIES[0],
  experienceYears: 1,
  about: '',
  isOnline: false,
  rating: 5,
  reviewsCount: 0,
});

export function AdminPharmacistsPage() {
  const [pharmacists, setPharmacists] = useState<Pharmacist[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [formData, setFormData] = useState<FormState>(emptyForm());
  const [saving, setSaving] = useState(false);

  async function loadList() {
    setLoading(true);
    setError(null);
    try {
      const paged = await pharmacistsApi.list({ page: 1, pageSize: 100 });
      setPharmacists(paged.items);
    } catch (err: unknown) {
      const e = err as { response?: { data?: { detail?: string } }; message?: string };
      setError(e.response?.data?.detail ?? e.message ?? 'Không tải được danh sách dược sĩ');
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadList();
  }, []);

  const handleOpenModal = (pharmacist?: Pharmacist) => {
    if (pharmacist) {
      setEditingId(pharmacist.id);
      setFormData({
        fullName: pharmacist.fullName,
        licenseNumber: pharmacist.licenseNumber,
        avatarUrl: pharmacist.avatarUrl ?? '',
        specialization: pharmacist.specialization ?? SPECIALTIES[0],
        experienceYears: pharmacist.experienceYears,
        about: pharmacist.about ?? '',
        isOnline: pharmacist.isOnline,
        rating: pharmacist.rating,
        reviewsCount: pharmacist.reviewsCount,
      });
    } else {
      setEditingId(null);
      setFormData(emptyForm());
    }
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setEditingId(null);
  };

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    const body: PharmacistUpsertRequest = {
      fullName: formData.fullName.trim(),
      licenseNumber: formData.licenseNumber.trim() || null,
      specialization: formData.specialization || null,
      phone: null,
      email: null,
      avatarUrl: formData.avatarUrl.trim() || null,
      isOnline: formData.isOnline,
      isActive: true,
      experienceYears: Number(formData.experienceYears) || 0,
      about: formData.about.trim() || null,
      rating: Number(formData.rating) || 0,
      reviewsCount: Number(formData.reviewsCount) || 0,
    };
    try {
      if (editingId) {
        await pharmacistsApi.update(editingId, { ...body, licenseNumber: body.licenseNumber ?? '' });
      } else {
        await pharmacistsApi.create(body);
      }
      handleCloseModal();
      await loadList();
    } catch (err: unknown) {
      const e = err as { response?: { data?: { detail?: string } }; message?: string };
      alert(e.response?.data?.detail ?? e.message ?? 'Không lưu được hồ sơ dược sĩ');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm('Bạn có chắc chắn muốn xóa dược sĩ này khỏi danh sách tư vấn không?')) return;
    try {
      await pharmacistsApi.delete(id);
      await loadList();
    } catch (err: unknown) {
      const e = err as { response?: { data?: { detail?: string } }; message?: string };
      alert(e.response?.data?.detail ?? e.message ?? 'Không xóa được hồ sơ dược sĩ');
    }
  };

  return (
    <AdminPageShell
      title="Quản lý Dược sĩ"
      description="Quản lý danh sách hồ sơ dược sĩ hiển thị trên trang Tư vấn trực tuyến."
      actions={
        <button
          onClick={() => handleOpenModal()}
          className="flex items-center gap-2 bg-primary text-on-primary px-4 py-2 rounded-xl font-semibold hover:bg-primary/90 transition-colors shadow-md"
        >
          <span className="material-symbols-outlined text-[20px]">person_add</span>
          Thêm Dược sĩ
        </button>
      }
    >
      <div className="bg-surface-container/60 border border-outline-variant/40 rounded-2xl overflow-hidden backdrop-blur ambient-shadow">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead className="bg-surface-container-high/50 text-on-surface-variant">
              <tr>
                <th className="px-6 py-4 font-semibold">Dược sĩ</th>
                <th className="px-6 py-4 font-semibold">Chuyên khoa</th>
                <th className="px-6 py-4 font-semibold">Kinh nghiệm</th>
                <th className="px-6 py-4 font-semibold">Đánh giá</th>
                <th className="px-6 py-4 font-semibold text-center">Trạng thái</th>
                <th className="px-6 py-4 font-semibold text-right">Thao tác</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-outline-variant/30 text-on-surface">
              {loading ? (
                <tr>
                  <td colSpan={6} className="px-6 py-8 text-center text-on-surface-variant">
                    Đang tải danh sách...
                  </td>
                </tr>
              ) : error ? (
                <tr>
                  <td colSpan={6} className="px-6 py-8 text-center text-error">
                    {error}
                  </td>
                </tr>
              ) : pharmacists.length === 0 ? (
                <tr>
                  <td colSpan={6} className="px-6 py-8 text-center text-on-surface-variant">
                    Chưa có hồ sơ dược sĩ nào.
                  </td>
                </tr>
              ) : (
                pharmacists.map((pharmacist) => (
                  <tr key={pharmacist.id} className="hover:bg-surface-container-high/30 transition-colors">
                    <td className="px-6 py-4">
                      <div className="flex items-center gap-3">
                        <img
                          src={pharmacist.avatarUrl ?? `https://i.pravatar.cc/150?u=p${pharmacist.id}`}
                          alt="avatar"
                          className="w-10 h-10 rounded-full object-cover border border-outline-variant/50"
                        />
                        <span className="font-semibold text-primary">{pharmacist.fullName}</span>
                      </div>
                    </td>
                    <td className="px-6 py-4">{pharmacist.specialization ?? '-'}</td>
                    <td className="px-6 py-4 text-on-surface-variant">{pharmacist.experienceYears} năm</td>
                    <td className="px-6 py-4">
                      <div className="flex items-center gap-1 text-amber-500">
                        <span className="material-symbols-outlined text-[16px] fill-current">star</span>
                        <span className="font-semibold text-on-surface">{Number(pharmacist.rating).toFixed(1)}</span>
                        <span className="text-on-surface-variant text-xs">({pharmacist.reviewsCount})</span>
                      </div>
                    </td>
                    <td className="px-6 py-4 text-center">
                      <span className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-semibold ${
                        pharmacist.isOnline
                          ? 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400'
                          : 'bg-surface-container-highest text-on-surface-variant'
                      }`}>
                        <span className={`w-1.5 h-1.5 rounded-full ${pharmacist.isOnline ? 'bg-green-500' : 'bg-on-surface-variant'}`} />
                        {pharmacist.isOnline ? 'Online' : 'Offline'}
                      </span>
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex items-center justify-end gap-2">
                        <button
                          onClick={() => handleOpenModal(pharmacist)}
                          className="p-2 text-on-surface-variant hover:text-primary hover:bg-primary-container/30 rounded-lg transition-colors"
                          title="Sửa"
                        >
                          <span className="material-symbols-outlined text-[20px]">edit</span>
                        </button>
                        <button
                          onClick={() => handleDelete(pharmacist.id)}
                          className="p-2 text-on-surface-variant hover:text-error hover:bg-error-container/30 rounded-lg transition-colors"
                          title="Xóa"
                        >
                          <span className="material-symbols-outlined text-[20px]">delete</span>
                        </button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {isModalOpen && (
        <div className="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-black/40 backdrop-blur-sm">
          <div className="bg-surface text-on-surface w-full max-w-lg rounded-3xl shadow-xl overflow-hidden border border-outline-variant/30 flex flex-col max-h-[90vh]">
            <div className="flex items-center justify-between px-6 py-4 border-b border-outline-variant/30">
              <h2 className="text-xl font-bold font-headline flex items-center gap-2">
                <span className="material-symbols-outlined text-primary">badge</span>
                {editingId ? 'Sửa hồ sơ Dược sĩ' : 'Thêm Dược sĩ mới'}
              </h2>
              <button
                onClick={handleCloseModal}
                className="p-2 text-on-surface-variant hover:bg-surface-container rounded-full transition-colors"
              >
                <span className="material-symbols-outlined text-[24px]">close</span>
              </button>
            </div>

            <div className="p-6 overflow-y-auto">
              <form id="pharmacist-form" onSubmit={handleSave} className="space-y-4">
                <div className="flex gap-4">
                  <div className="flex-1">
                    <label className="block text-sm font-semibold mb-1.5 text-on-surface">Tên Dược sĩ <span className="text-error">*</span></label>
                    <input
                      type="text"
                      required
                      value={formData.fullName}
                      onChange={(e) => setFormData({ ...formData, fullName: e.target.value })}
                      placeholder="VD: DS. Nguyễn Văn A"
                      className="w-full px-4 py-2.5 rounded-xl border border-outline-variant bg-surface focus:outline-none focus:ring-2 focus:ring-primary/50 text-sm"
                    />
                  </div>
                  <div className="w-32">
                    <label className="block text-sm font-semibold mb-1.5 text-on-surface">Kinh nghiệm</label>
                    <div className="relative">
                      <input
                        type="number"
                        min="0"
                        required
                        value={formData.experienceYears}
                        onChange={(e) => setFormData({ ...formData, experienceYears: Number(e.target.value) })}
                        className="w-full px-4 py-2.5 pr-10 rounded-xl border border-outline-variant bg-surface focus:outline-none focus:ring-2 focus:ring-primary/50 text-sm"
                      />
                      <span className="absolute right-3 top-1/2 -translate-y-1/2 text-sm text-on-surface-variant">năm</span>
                    </div>
                  </div>
                </div>

                <div>
                  <label className="block text-sm font-semibold mb-1.5 text-on-surface">Số giấy phép {editingId ? <span className="text-error">*</span> : <span className="text-on-surface-variant text-xs">(để trống = tự sinh)</span>}</label>
                  <input
                    type="text"
                    required={!!editingId}
                    value={formData.licenseNumber}
                    onChange={(e) => setFormData({ ...formData, licenseNumber: e.target.value })}
                    placeholder="VD: PH-20260516-1234"
                    className="w-full px-4 py-2.5 rounded-xl border border-outline-variant bg-surface focus:outline-none focus:ring-2 focus:ring-primary/50 text-sm"
                  />
                </div>

                <div>
                  <label className="block text-sm font-semibold mb-1.5 text-on-surface">Chuyên khoa chính</label>
                  <select
                    value={formData.specialization}
                    onChange={(e) => setFormData({ ...formData, specialization: e.target.value })}
                    className="w-full px-4 py-2.5 rounded-xl border border-outline-variant bg-surface focus:outline-none focus:ring-2 focus:ring-primary/50 text-sm appearance-none cursor-pointer"
                  >
                    {SPECIALTIES.map(s => <option key={s} value={s}>{s}</option>)}
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-semibold mb-1.5 text-on-surface">Giới thiệu (About)</label>
                  <textarea
                    rows={3}
                    required
                    value={formData.about}
                    onChange={(e) => setFormData({ ...formData, about: e.target.value })}
                    placeholder="Mô tả chuyên môn, kinh nghiệm nổi bật..."
                    className="w-full px-4 py-2.5 rounded-xl border border-outline-variant bg-surface focus:outline-none focus:ring-2 focus:ring-primary/50 text-sm resize-none"
                  ></textarea>
                </div>

                <div>
                  <label className="block text-sm font-semibold mb-1.5 text-on-surface">URL Ảnh đại diện</label>
                  <input
                    type="url"
                    required
                    value={formData.avatarUrl}
                    onChange={(e) => setFormData({ ...formData, avatarUrl: e.target.value })}
                    className="w-full px-4 py-2.5 rounded-xl border border-outline-variant bg-surface focus:outline-none focus:ring-2 focus:ring-primary/50 text-sm"
                  />
                  {formData.avatarUrl && (
                    <div className="mt-2 flex items-center gap-3">
                      <span className="text-xs text-on-surface-variant">Preview:</span>
                      <img src={formData.avatarUrl} alt="preview" className="w-8 h-8 rounded-full object-cover border border-outline-variant" />
                    </div>
                  )}
                </div>

                <div className="flex gap-4">
                  <div className="flex-1">
                    <label className="block text-sm font-semibold mb-1.5 text-on-surface">Rating (0-5)</label>
                    <input
                      type="number"
                      min="0"
                      max="5"
                      step="0.1"
                      value={formData.rating}
                      onChange={(e) => setFormData({ ...formData, rating: Number(e.target.value) })}
                      className="w-full px-4 py-2.5 rounded-xl border border-outline-variant bg-surface focus:outline-none focus:ring-2 focus:ring-primary/50 text-sm"
                    />
                  </div>
                  <div className="flex-1">
                    <label className="block text-sm font-semibold mb-1.5 text-on-surface">Số lượt đánh giá</label>
                    <input
                      type="number"
                      min="0"
                      value={formData.reviewsCount}
                      onChange={(e) => setFormData({ ...formData, reviewsCount: Number(e.target.value) })}
                      className="w-full px-4 py-2.5 rounded-xl border border-outline-variant bg-surface focus:outline-none focus:ring-2 focus:ring-primary/50 text-sm"
                    />
                  </div>
                </div>

                <div className="flex items-center gap-3 pt-2">
                  <label className="relative inline-flex items-center cursor-pointer">
                    <input
                      type="checkbox"
                      className="sr-only peer"
                      checked={formData.isOnline}
                      onChange={(e) => setFormData({ ...formData, isOnline: e.target.checked })}
                    />
                    <div className="w-11 h-6 bg-surface-container-high peer-focus:outline-none peer-focus:ring-2 peer-focus:ring-primary/50 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-green-500"></div>
                    <span className="ml-3 text-sm font-semibold text-on-surface">Đang Online (Sẵn sàng chat)</span>
                  </label>
                </div>
              </form>
            </div>

            <div className="px-6 py-4 bg-surface-container/30 border-t border-outline-variant/30 flex justify-end gap-3">
              <button
                type="button"
                onClick={handleCloseModal}
                disabled={saving}
                className="px-5 py-2.5 rounded-xl font-semibold text-on-surface-variant hover:bg-surface-container-high transition-colors disabled:opacity-50"
              >
                Hủy
              </button>
              <button
                type="submit"
                form="pharmacist-form"
                disabled={saving}
                className="px-6 py-2.5 rounded-xl font-semibold bg-primary text-on-primary hover:bg-primary/90 shadow-md shadow-primary/20 transition-all disabled:opacity-60"
              >
                {saving ? 'Đang lưu...' : 'Lưu hồ sơ'}
              </button>
            </div>
          </div>
        </div>
      )}
    </AdminPageShell>
  );
}
