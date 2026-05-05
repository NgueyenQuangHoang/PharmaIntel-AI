import { useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '@/hooks/redux';
import { fetchFeaturedThunk } from '@/features/medications/medications-slice';
import { addToCartThunk } from '@/features/cart/cart-slice';
import { formatVnd } from '@/utils/format';
import type { MedicationListItem } from '@/features/medications/types';

const FALLBACK_IMG = 'https://lh3.googleusercontent.com/aida-public/AB6AXuD0sEiq2tcqiVyVMMFPcvVFkJtl7swdj7Yy4JxdAC4heRbkv3vZAg7ACX0Jj7st_Ygx7AAAIhcqZczL8laZaLFoD8FFp68n-xrc_U1aWrrHVBy7m4zGGK0oT3iaKGFvxLFALirIHBsWm0lwEqpqYlnka1BnRNdJrU9mV3sXbyAKysbwGJCZB9_93rwqA3XV1FDe-Z0bZyjv8s56ctDHbkVzY1H4Z8DfEgjaClDGHDRSL6b9n-EdyL6-r1-nrFESjx9LarBpy6eZXn4';

function FeatureBigCard({ med, onAdd }: { med: MedicationListItem; onAdd: () => void }) {
  return (
    <div className="md:col-span-8 bg-surface-container-lowest rounded-3xl p-8 border border-outline-variant/15 flex flex-col md:flex-row gap-8 items-center overflow-hidden">
      <div className="flex-1 space-y-6">
        <span className="px-3 py-1 bg-tertiary-fixed text-on-tertiary-fixed text-xs font-bold rounded-full uppercase tracking-widest">
          {med.isBestSeller ? 'Bán chạy nhất' : 'Nổi bật'}
        </span>
        <h3 className="text-3xl font-bold font-headline text-on-surface">{med.name}</h3>
        <p className="text-on-surface-variant">
          {med.manufacturer ?? med.genericName ?? 'Sản phẩm dược phẩm chất lượng cao.'}
        </p>
        <div className="flex items-center gap-4">
          <span className="text-2xl font-bold text-primary">{formatVnd(med.finalPrice)}</span>
          <button
            onClick={onAdd}
            className="bg-primary text-on-primary px-6 py-2 rounded-full font-semibold hover:opacity-90 transition-opacity"
          >
            Thêm vào giỏ
          </button>
        </div>
      </div>
      <div className="flex-1">
        <img
          alt={med.name}
          className="w-full h-64 object-contain"
          src={med.imageUrl || FALLBACK_IMG}
        />
      </div>
    </div>
  );
}

function FeatureSmallCard({ med, onAdd }: { med: MedicationListItem; onAdd: () => void }) {
  return (
    <div className="md:col-span-4 bg-surface-container-lowest rounded-3xl p-8 border border-outline-variant/15 relative overflow-hidden">
      {med.discountPercent > 0 && (
        <div className="absolute top-0 right-0 p-4">
          <span className="px-2 py-1 bg-error-container text-on-error-container text-[10px] font-bold rounded-lg uppercase">
            -{med.discountPercent}%
          </span>
        </div>
      )}
      <img
        alt={med.name}
        className="w-full h-40 object-contain mb-6"
        src={med.imageUrl || FALLBACK_IMG}
      />
      <h4 className="text-xl font-bold font-headline text-on-surface mb-2 line-clamp-1">
        {med.name}
      </h4>
      <div className="flex justify-between items-center">
        <span className="font-bold text-primary">{formatVnd(med.finalPrice)}</span>
        <button onClick={onAdd} className="text-outline hover:text-primary transition-colors">
          <span className="material-symbols-outlined">add_circle</span>
        </button>
      </div>
    </div>
  );
}

export function FeaturedMedications() {
  const dispatch = useAppDispatch();
  const { items, status, error } = useAppSelector((s) => s.medications.featured);
  const isAuthenticated = useAppSelector(
    (s) => s.auth.status === 'authenticated' && !!s.auth.token,
  );

  useEffect(() => {
    if (status === 'idle') {
      dispatch(fetchFeaturedThunk());
    }
  }, [status, dispatch]);

  const handleAdd = (medicationId: number) => {
    if (!isAuthenticated) return;
    dispatch(addToCartThunk({ medicationId, quantity: 1 }));
  };

  const big = items[0];
  const smalls = items.slice(1, 3);

  return (
    <section className="bg-surface py-24 px-8">
      <div className="max-w-7xl mx-auto">
        <div className="flex flex-col md:flex-row justify-between items-end mb-16 gap-6">
          <div className="max-w-2xl">
            <h2 className="text-4xl font-extrabold font-headline text-on-surface mb-4 tracking-tight">
              Tủ thuốc Nổi bật
            </h2>
            <p className="text-on-surface-variant leading-relaxed">
              Dược phẩm được kiểm định chất lượng nghiêm ngặt bởi Hội đồng Y khoa Quốc tế.
            </p>
          </div>
          <Link
            to="/medicine"
            className="text-primary font-bold flex items-center gap-2 group"
          >
            Xem tất cả danh mục{' '}
            <span className="material-symbols-outlined group-hover:translate-x-1 transition-transform">
              arrow_forward
            </span>
          </Link>
        </div>

        {status === 'loading' && (
          <div className="text-center text-on-surface-variant py-20">Đang tải...</div>
        )}
        {status === 'error' && (
          <div className="rounded-lg bg-error-container/40 border border-error/30 px-4 py-3 text-sm text-error">
            {error ?? 'Không tải được sản phẩm nổi bật'}
          </div>
        )}

        {status === 'success' && (
          <div className="grid grid-cols-1 md:grid-cols-12 gap-6">
            {big && <FeatureBigCard med={big} onAdd={() => handleAdd(big.id)} />}

            {/* Card phụ: link sang medicine cabinet */}
            <div className="md:col-span-4 bg-secondary-fixed text-on-secondary-fixed rounded-3xl p-8 flex flex-col justify-between">
              <div className="space-y-4">
                <span
                  className="material-symbols-outlined text-4xl"
                  style={{ fontVariationSettings: "'FILL' 1" }}
                >
                  spa
                </span>
                <h3 className="text-2xl font-bold font-headline">Dược liệu Tự nhiên</h3>
                <p className="text-on-secondary-fixed-variant opacity-80">
                  Chiết xuất 100% thảo dược organic.
                </p>
              </div>
              <Link
                to="/medicine"
                className="mt-8 w-full py-3 bg-on-secondary-fixed text-white rounded-xl font-bold text-center"
              >
                Khám phá ngay
              </Link>
            </div>

            {smalls.map((med) => (
              <FeatureSmallCard key={med.id} med={med} onAdd={() => handleAdd(med.id)} />
            ))}

            {/* Tile dan toi danh muc */}
            <Link
              to="/medicine"
              className="md:col-span-4 bg-surface-container-low rounded-3xl p-8 border border-outline-variant/10 group cursor-pointer hover:bg-primary transition-colors"
            >
              <div className="h-full flex flex-col justify-center items-center text-center space-y-4">
                <div className="w-16 h-16 rounded-full bg-white flex items-center justify-center text-primary group-hover:scale-110 transition-transform">
                  <span className="material-symbols-outlined text-3xl">category</span>
                </div>
                <h4 className="text-xl font-bold font-headline group-hover:text-white">
                  Tất cả sản phẩm
                </h4>
              </div>
            </Link>
          </div>
        )}
      </div>
    </section>
  );
}
