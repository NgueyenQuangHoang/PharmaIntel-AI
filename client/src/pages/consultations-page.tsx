// =============================================================================
// ConsultationsPage - Trang Tu Van Duoc Si (load danh sach tu API /pharmacists).
// =============================================================================
import { useEffect, useMemo, useState } from 'react';
import axios from 'axios';
import { pharmacistsApi } from '@/features/pharmacists/pharmacists-api';
import type { Pharmacist } from '@/features/pharmacists/types';
import { consultationsApi } from '@/features/consultations/consultations-api';

const SPECIALTIES = ['Tất cả', 'Dược lâm sàng', 'Dược liệu & Cổ truyền', 'Dinh dưỡng', 'Dược lý'];

export function ConsultationsPage() {
  const [pharmacists, setPharmacists] = useState<Pharmacist[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [searchTerm, setSearchTerm] = useState('');
  const [selectedSpecialty, setSelectedSpecialty] = useState('Tất cả');
  const [selectedPharmacist, setSelectedPharmacist] = useState<Pharmacist | null>(null);
  const [isBookingModalOpen, setIsBookingModalOpen] = useState(false);
  const [bookingForm, setBookingForm] = useState({ date: '', time: '', note: '' });
  const [bookingError, setBookingError] = useState<string | null>(null);
  const [bookingSubmitting, setBookingSubmitting] = useState(false);
  const [bookingToast, setBookingToast] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      setLoading(true);
      setError(null);
      try {
        const paged = await pharmacistsApi.list({
          page: 1,
          pageSize: 100,
          isActive: true,
        });
        if (!cancelled) setPharmacists(paged.items);
      } catch (err: unknown) {
        const e = err as { response?: { data?: { detail?: string } }; message?: string };
        if (!cancelled) setError(e.response?.data?.detail ?? e.message ?? 'Không tải được danh sách dược sĩ');
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, []);

  const filteredPharmacists = useMemo(() => {
    return pharmacists.filter((p) => {
      const matchName = p.fullName.toLowerCase().includes(searchTerm.toLowerCase());
      const matchSpecialty = selectedSpecialty === 'Tất cả' || p.specialization === selectedSpecialty;
      return matchName && matchSpecialty;
    });
  }, [pharmacists, searchTerm, selectedSpecialty]);

  const handleOpenBooking = (pharmacist: Pharmacist) => {
    setSelectedPharmacist(pharmacist);
    setIsBookingModalOpen(true);
  };

  const handleCloseBooking = () => {
    setIsBookingModalOpen(false);
    setSelectedPharmacist(null);
    setBookingForm({ date: '', time: '', note: '' });
    setBookingError(null);
    setBookingSubmitting(false);
  };

  const handleConfirmBooking = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedPharmacist) return;
    setBookingError(null);

    // Gop date + time thanh ISO theo gio local roi gui len server.
    const scheduledAt = new Date(`${bookingForm.date}T${bookingForm.time}:00`);
    if (Number.isNaN(scheduledAt.getTime())) {
      setBookingError('Ngày hoặc giờ không hợp lệ.');
      return;
    }
    if (scheduledAt.getTime() <= Date.now()) {
      setBookingError('Thời gian tư vấn phải sau thời điểm hiện tại.');
      return;
    }

    setBookingSubmitting(true);
    try {
      await consultationsApi.create({
        pharmacistId: selectedPharmacist.id,
        scheduledAt: scheduledAt.toISOString(),
        note: bookingForm.note.trim() || null,
      });
      const name = selectedPharmacist.fullName;
      handleCloseBooking();
      setBookingToast(`Đã gửi yêu cầu tư vấn tới ${name}. Dược sĩ sẽ phản hồi sớm.`);
      window.setTimeout(() => setBookingToast(null), 4000);
    } catch (err: unknown) {
      let message = 'Không gửi được yêu cầu, vui lòng thử lại.';
      if (axios.isAxiosError(err)) {
        const data = err.response?.data as { detail?: string; title?: string; message?: string } | undefined;
        if (err.response?.status === 409) {
          message = data?.detail ?? 'Khung giờ này đã có lịch tư vấn khác, vui lòng chọn thời gian khác.';
        } else {
          message = data?.detail ?? data?.title ?? data?.message ?? err.message ?? message;
        }
      }
      setBookingError(message);
      setBookingSubmitting(false);
    }
  };

  return (
    <div className="min-h-screen bg-surface pb-20">
      {/* Hero Section */}
      <section className="relative bg-blue-50 py-16 px-6 lg:px-8 overflow-hidden">
        <div className="absolute inset-0 bg-[url('/consultation-banner.jpg')] bg-cover bg-center bg-no-repeat opacity-40 mix-blend-multiply"></div>
        <div className="max-w-5xl mx-auto relative z-10 text-center">
          <h1 className="text-4xl md:text-5xl font-bold font-headline text-on-surface mb-4">
            Tư Vấn Trực Tuyến Cùng Dược Sĩ
          </h1>
          <p className="text-lg md:text-xl text-on-surface-variant max-w-2xl mx-auto mb-8">
            Kết nối ngay với đội ngũ dược sĩ chuyên môn cao để được giải đáp thắc mắc về sức khỏe và hướng dẫn dùng thuốc an toàn.
          </p>

          <div className="bg-surface/80 backdrop-blur-xl p-4 rounded-3xl shadow-lg border border-outline-variant/30 flex flex-col md:flex-row gap-4 items-center justify-between max-w-3xl mx-auto">
            <div className="relative flex-1 w-full">
              <span className="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-on-surface-variant">
                search
              </span>
              <input
                type="text"
                placeholder="Tìm tên dược sĩ..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full bg-surface-container-highest/50 border border-outline-variant/50 rounded-2xl py-3 pl-12 pr-4 focus:outline-none focus:ring-2 focus:ring-primary/50 text-on-surface transition-all"
              />
            </div>
            <div className="flex-shrink-0 w-full md:w-auto">
              <select
                value={selectedSpecialty}
                onChange={(e) => setSelectedSpecialty(e.target.value)}
                className="w-full md:w-48 bg-surface-container-highest/50 border border-outline-variant/50 rounded-2xl py-3 px-4 focus:outline-none focus:ring-2 focus:ring-primary/50 text-on-surface transition-all appearance-none cursor-pointer"
              >
                {SPECIALTIES.map((spec) => (
                  <option key={spec} value={spec}>{spec}</option>
                ))}
              </select>
            </div>
          </div>
        </div>
      </section>

      {/* Pharmacist List */}
      <section className="max-w-7xl mx-auto px-6 lg:px-8 py-12">
        <div className="mb-6 flex items-center justify-between">
          <h2 className="text-2xl font-bold font-headline text-on-surface">
            Danh sách Dược sĩ ({filteredPharmacists.length})
          </h2>
        </div>

        {loading ? (
          <div className="text-center py-20 text-on-surface-variant">Đang tải danh sách dược sĩ...</div>
        ) : error ? (
          <div className="text-center py-20 bg-surface-container/50 rounded-3xl border border-outline-variant/30 text-error">
            {error}
          </div>
        ) : filteredPharmacists.length === 0 ? (
          <div className="text-center py-20 bg-surface-container/50 rounded-3xl border border-outline-variant/30">
            <span className="material-symbols-outlined text-6xl text-on-surface-variant/50 mb-4 block">
              person_search
            </span>
            <p className="text-on-surface-variant text-lg">Không tìm thấy dược sĩ nào phù hợp với điều kiện tìm kiếm.</p>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {filteredPharmacists.map((pharmacist) => (
              <div
                key={pharmacist.id}
                className="bg-surface-container/40 hover:bg-surface-container/80 transition-all duration-300 rounded-3xl p-6 border border-outline-variant/30 ambient-shadow group flex flex-col"
              >
                <div className="flex items-start gap-4 mb-4">
                  <div className="relative">
                    <img
                      src={pharmacist.avatarUrl ?? `https://i.pravatar.cc/150?u=p${pharmacist.id}`}
                      alt={pharmacist.fullName}
                      className="w-20 h-20 rounded-full object-cover border-4 border-surface shadow-sm group-hover:scale-105 transition-transform"
                    />
                    {pharmacist.isOnline && (
                      <span className="absolute bottom-1 right-1 w-4 h-4 bg-green-500 border-2 border-surface rounded-full shadow-sm" title="Đang hoạt động"></span>
                    )}
                  </div>
                  <div className="flex-1">
                    <h3 className="text-lg font-bold text-on-surface font-headline leading-tight">
                      {pharmacist.fullName}
                    </h3>
                    <p className="text-sm text-primary font-semibold mt-1">{pharmacist.specialization ?? 'Đang cập nhật'}</p>
                    <div className="flex items-center gap-1 mt-2 text-sm text-on-surface-variant">
                      <span className="material-symbols-outlined text-[16px] text-amber-500 fill-current">star</span>
                      <span className="font-semibold text-on-surface">{Number(pharmacist.rating).toFixed(1)}</span>
                      <span>({pharmacist.reviewsCount} đánh giá)</span>
                    </div>
                  </div>
                </div>

                <p className="text-sm text-on-surface-variant mb-6 line-clamp-3 flex-1">
                  {pharmacist.about ?? 'Hồ sơ dược sĩ đang được cập nhật.'}
                </p>

                <div className="flex items-center gap-4 mt-auto pt-4 border-t border-outline-variant/30">
                  <div className="flex-1 text-center border-r border-outline-variant/30">
                    <p className="text-xs text-on-surface-variant">Kinh nghiệm</p>
                    <p className="font-semibold text-on-surface">{pharmacist.experienceYears} năm</p>
                  </div>
                  <div className="flex-[2] flex justify-end gap-2">
                    <button
                      onClick={() => alert('Tính năng chat trực tiếp đang được phát triển.')}
                      className="p-2.5 rounded-xl bg-secondary-container text-secondary hover:bg-secondary/20 transition-colors"
                      title="Chat nhanh"
                    >
                      <span className="material-symbols-outlined">chat</span>
                    </button>
                    <button
                      onClick={() => handleOpenBooking(pharmacist)}
                      className="flex-1 py-2.5 rounded-xl bg-primary text-on-primary font-semibold shadow-md hover:bg-primary/90 hover:shadow-lg transition-all"
                    >
                      Đặt lịch
                    </button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </section>

      {bookingToast && (
        <div className="fixed top-6 right-6 z-50 max-w-sm p-4 rounded-2xl bg-primary text-on-primary shadow-lg font-semibold text-sm animate-in fade-in slide-in-from-top-4">
          {bookingToast}
        </div>
      )}

      {/* Booking Modal */}
      {isBookingModalOpen && selectedPharmacist && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
          <div className="bg-surface text-on-surface w-full max-w-md rounded-3xl shadow-2xl overflow-hidden border border-outline-variant/30 flex flex-col transform transition-all">
            <div className="p-6 border-b border-outline-variant/30 bg-surface-container/30">
              <h2 className="text-xl font-bold font-headline flex items-center gap-2">
                <span className="material-symbols-outlined text-primary">calendar_clock</span>
                Đặt lịch tư vấn
              </h2>
              <p className="text-sm text-on-surface-variant mt-1">
                với <strong className="text-on-surface">{selectedPharmacist.fullName}</strong>
              </p>
            </div>

            <div className="p-6">
              {bookingError && (
                <div className="mb-4 p-3 rounded-xl bg-error-container text-on-error-container text-sm font-medium">
                  {bookingError}
                </div>
              )}
              <form id="booking-form" onSubmit={handleConfirmBooking} className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-semibold mb-1.5 text-on-surface">Ngày tư vấn <span className="text-error">*</span></label>
                    <input
                      type="date"
                      required
                      min={new Date().toISOString().split('T')[0]}
                      value={bookingForm.date}
                      onChange={(e) => setBookingForm({ ...bookingForm, date: e.target.value })}
                      className="w-full px-4 py-2.5 rounded-xl border border-outline-variant bg-surface focus:outline-none focus:ring-2 focus:ring-primary/50 text-sm"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-semibold mb-1.5 text-on-surface">Giờ (Dự kiến) <span className="text-error">*</span></label>
                    <input
                      type="time"
                      required
                      value={bookingForm.time}
                      onChange={(e) => setBookingForm({ ...bookingForm, time: e.target.value })}
                      className="w-full px-4 py-2.5 rounded-xl border border-outline-variant bg-surface focus:outline-none focus:ring-2 focus:ring-primary/50 text-sm"
                    />
                  </div>
                </div>

                <div>
                  <label className="block text-sm font-semibold mb-1.5 text-on-surface">Vấn đề cần tư vấn</label>
                  <textarea
                    rows={3}
                    placeholder="Mô tả ngắn gọn triệu chứng hoặc câu hỏi của bạn..."
                    value={bookingForm.note}
                    onChange={(e) => setBookingForm({ ...bookingForm, note: e.target.value })}
                    className="w-full px-4 py-2.5 rounded-xl border border-outline-variant bg-surface focus:outline-none focus:ring-2 focus:ring-primary/50 text-sm resize-none"
                  ></textarea>
                </div>
              </form>
            </div>

            <div className="px-6 py-4 bg-surface-container/30 border-t border-outline-variant/30 flex justify-end gap-3">
              <button
                type="button"
                onClick={handleCloseBooking}
                disabled={bookingSubmitting}
                className="px-5 py-2.5 rounded-xl font-semibold text-on-surface-variant hover:bg-surface-container-high transition-colors disabled:opacity-50"
              >
                Hủy
              </button>
              <button
                type="submit"
                form="booking-form"
                disabled={bookingSubmitting}
                className="px-6 py-2.5 rounded-xl font-semibold bg-primary text-on-primary hover:bg-primary/90 shadow-md shadow-primary/20 transition-all disabled:opacity-60 disabled:cursor-not-allowed"
              >
                {bookingSubmitting ? 'Đang gửi...' : 'Xác nhận'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
