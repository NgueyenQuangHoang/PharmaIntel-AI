export function StatsSection() {
  return (
    <section className="bg-surface py-16 px-8">
      <div className="max-w-7xl mx-auto grid grid-cols-2 md:grid-cols-4 gap-8">
        <div className="text-center p-8 bg-surface-container-low rounded-xl">
          <div className="text-4xl font-extrabold font-headline text-primary mb-2">5M+</div>
          <div className="text-sm font-medium text-on-surface-variant">Người dùng tin tưởng</div>
        </div>
        <div className="text-center p-8 bg-surface-container-low rounded-xl">
          <div className="text-4xl font-extrabold font-headline text-primary mb-2">12k+</div>
          <div className="text-sm font-medium text-on-surface-variant">Bác sĩ chuyên khoa</div>
        </div>
        <div className="text-center p-8 bg-surface-container-low rounded-xl">
          <div className="text-4xl font-extrabold font-headline text-primary mb-2">500+</div>
          <div className="text-sm font-medium text-on-surface-variant">Loại bệnh lý hỗ trợ</div>
        </div>
        <div className="text-center p-8 bg-surface-container-low rounded-xl">
          <div className="text-4xl font-extrabold font-headline text-primary mb-2">24/7</div>
          <div className="text-sm font-medium text-on-surface-variant">Hỗ trợ trực tuyến</div>
        </div>
      </div>
    </section>
  );
}
