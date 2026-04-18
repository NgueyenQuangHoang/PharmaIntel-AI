export function SuggestedMedications() {
  return (
    <div className="space-y-8">
      <h2 className="text-2xl font-headline font-bold text-on-surface flex items-center">
        <span className="material-symbols-outlined mr-2 text-primary" style={{ fontVariationSettings: "'FILL' 1" }}>medication</span>
        Các loại thuốc được đề xuất
      </h2>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {/* Card 1 */}
        <article className="bg-surface-container-lowest rounded-xl overflow-hidden shadow-sm hover:shadow-md transition-shadow group border border-outline-variant/15">
          <div className="aspect-video overflow-hidden bg-surface-container-low">
            <img alt="Paracetamol" className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500" data-alt="..." src="https://lh3.googleusercontent.com/aida-public/AB6AXuBfZQ0ruQS33ro97Il1I21w9G1WjuJFgbv8KSVPHefZMNSmRGIYAVOVZ0Wvbodxy4H7VDECSPrs0eSPnlqxqP8Ex6vyXgD9jzdzvaKeCF5WVnSInhxdYI1n3L-8ITV0uNjWap14nU0dUXB5gYZu1akgyrDoZbJdaTJIOZKWcwhY7r7CVzK65iUXWzZvWpwJO6l_A8qh-CGbM_efHBQb2OESEaCeJbAfXmDDi42ccgDzosiHUsZaGJajFvsmAcRrbGFxdQZHXSazFz8" />
          </div>
          <div className="p-6">
            <div className="flex justify-between items-start mb-4">
              <div>
                <h3 className="font-headline text-xl font-bold text-on-surface">Paracetamol 500mg</h3>
                <span className="text-xs font-semibold text-primary uppercase tracking-wider">Hạ sốt &amp; Giảm đau</span>
              </div>
              <div className="text-right">
                <div className="text-lg font-bold text-on-surface">45.000đ</div>
                <div className="text-xs text-on-surface-variant">Hộp 20 viên</div>
              </div>
            </div>
            <div className="bg-surface-container-low rounded-lg p-4 mb-6">
              <div className="flex items-center text-sm font-semibold mb-2">
                <span className="material-symbols-outlined text-sm mr-2 text-secondary">info</span>
                Hướng dẫn sử dụng
              </div>
              <p className="text-sm text-on-surface-variant leading-snug">Uống 1-2 viên mỗi 4-6 giờ khi cần thiết. Không quá 8 viên trong 24 giờ.</p>
            </div>
            <button className="w-full py-3 bg-gradient-to-r from-primary to-primary-container text-on-primary font-bold rounded-full hover:opacity-90 transition-opacity flex items-center justify-center gap-2">
              <span className="material-symbols-outlined" style={{ fontVariationSettings: "'FILL' 1" }}>shopping_cart</span>
              Mua ngay
            </button>
          </div>
        </article>

        {/* Card 2 */}
        <article className="bg-surface-container-lowest rounded-xl overflow-hidden shadow-sm hover:shadow-md transition-shadow group border border-outline-variant/15">
          <div className="aspect-video overflow-hidden bg-surface-container-low">
            <img alt="Vitamin C Complex" className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500" data-alt="..." src="https://lh3.googleusercontent.com/aida-public/AB6AXuCc11ZVLCGydBU7zFgVUQk1mG59-N21sVb0bc5FWJXDhZsm7Y4uh3ieYdgl7Ma8ZYwtruqHh7Ytf14lctChwIS99jQ6zYZQ5wP0y8wKbfkySLZaVEj_3BghGWFpHxn8UDMHlNYcIdiUNNwbhTgUNu6YYbe8Jb6gRpBulXAMQbc2_aPaPsqmZnf9tqGHrrrumzPLpL5L59-00EYJmtc14DpQtmAHQpTPql4T1bsFjYKFz7DndRbCbL9Ph1HjNTF1feT5BnEJgjs23Yg" />
          </div>
          <div className="p-6">
            <div className="flex justify-between items-start mb-4">
              <div>
                <h3 className="font-headline text-xl font-bold text-on-surface">Vitamin C Plus</h3>
                <span className="text-xs font-semibold text-secondary uppercase tracking-wider">Tăng sức đề kháng</span>
              </div>
              <div className="text-right">
                <div className="text-lg font-bold text-on-surface">120.000đ</div>
                <div className="text-xs text-on-surface-variant">Lọ 30 viên sủi</div>
              </div>
            </div>
            <div className="bg-surface-container-low rounded-lg p-4 mb-6">
              <div className="flex items-center text-sm font-semibold mb-2">
                <span className="material-symbols-outlined text-sm mr-2 text-secondary">info</span>
                Hướng dẫn sử dụng
              </div>
              <p className="text-sm text-on-surface-variant leading-snug">Hòa tan 1 viên vào 200ml nước, uống sau bữa sáng để tăng hiệu quả hấp thu.</p>
            </div>
            <button className="w-full py-3 bg-gradient-to-r from-primary to-primary-container text-on-primary font-bold rounded-full hover:opacity-90 transition-opacity flex items-center justify-center gap-2">
              <span className="material-symbols-outlined" style={{ fontVariationSettings: "'FILL' 1" }}>shopping_cart</span>
              Mua ngay
            </button>
          </div>
        </article>

        {/* Card 3 (Featured/Large) */}
        <article className="md:col-span-2 bg-surface-container-lowest rounded-xl overflow-hidden shadow-sm hover:shadow-md transition-shadow group border border-outline-variant/15 flex flex-col md:flex-row">
          <div className="md:w-1/3 aspect-square md:aspect-auto overflow-hidden bg-surface-container-low">
            <img alt="Oresol" className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500" data-alt="..." src="https://lh3.googleusercontent.com/aida-public/AB6AXuA0uYOsHp9QzTbrXCQ92gckNbnVumu46jcHULLImyWknqZzYuEjjRmvvYx3SsuvqX4M54s_yCpb9bsAgRVKV-Yqm5M25JShKGhDZPqnb-F09pgtqig6kQ5OCwIjpDP_hbHl6LOhUdmLKQS8tRgwu1sPTM4vBesP0dDjQEElDBHFLhbbmtWJNkBRLIjZhg8FXqosxsLwSbpMMA1ac5ugoPEtFLuHrKUz_YJk26txN3LmmLft6Nd3Q9JUs71HYTFABuIX1oaXyx7bHrs" />
          </div>
          <div className="p-8 flex-1 flex flex-col justify-between">
            <div>
              <div className="flex justify-between items-start mb-4">
                <div>
                  <h3 className="font-headline text-2xl font-bold text-on-surface">Oresol Hydration Pro</h3>
                  <span className="text-xs font-semibold text-tertiary-container text-on-tertiary-container px-2 py-1 rounded bg-tertiary-fixed uppercase tracking-wider inline-block mt-1">Bù nước &amp; Điện giải</span>
                </div>
                <div className="text-right">
                  <div className="text-2xl font-bold text-on-surface">15.000đ</div>
                  <div className="text-sm text-on-surface-variant">Gói lẻ 10g</div>
                </div>
              </div>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-8">
                <div className="bg-surface-container-low rounded-lg p-4">
                  <div className="flex items-center text-sm font-semibold mb-1 text-primary">
                    <span className="material-symbols-outlined text-sm mr-2" style={{ fontVariationSettings: "'FILL' 1" }}>check_circle</span>
                    Công dụng
                  </div>
                  <p className="text-sm text-on-surface-variant">Ngăn ngừa mất nước do sốt cao và mệt mỏi kéo dài.</p>
                </div>
                <div className="bg-surface-container-low rounded-lg p-4">
                  <div className="flex items-center text-sm font-semibold mb-1 text-primary">
                    <span className="material-symbols-outlined text-sm mr-2">schedule</span>
                    Cách dùng
                  </div>
                  <p className="text-sm text-on-surface-variant">Pha 1 gói với đúng 1 lít nước đun sôi để nguội. Uống rải rác trong ngày.</p>
                </div>
              </div>
            </div>
            <button className="w-full py-4 bg-gradient-to-r from-primary to-primary-container text-on-primary font-bold rounded-full hover:opacity-90 transition-opacity flex items-center justify-center gap-2">
              <span className="material-symbols-outlined" style={{ fontVariationSettings: "'FILL' 1" }}>shopping_cart</span>
              Mua ngay
            </button>
          </div>
        </article>
      </div>
    </div>
  );
}
