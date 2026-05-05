interface MedicineHeaderProps {
  searchInput: string;
  onSearchChange: (val: string) => void;
  sortOption: string;
  onSortChange: (val: string) => void;
}

export function MedicineHeader({
  searchInput,
  onSearchChange,
  sortOption,
  onSortChange,
}: MedicineHeaderProps) {
  return (
    <>
      <div className="mb-12">
        <h1 className="font-headline text-5xl font-extrabold tracking-tight text-on-surface mb-4">Tủ thuốc Gia đình</h1>
        <p className="text-on-surface-variant text-lg max-w-2xl">Quản lý và tìm kiếm các loại dược phẩm thiết yếu với sự hỗ trợ từ trí tuệ nhân tạo lâm sàng.</p>
      </div>

      <div className="flex flex-col md:flex-row gap-6 mb-12 items-center">
        <div className="relative flex-1 w-full">
          <span className="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-outline">search</span>
          <input
            className="w-full pl-12 pr-4 py-4 bg-surface-container-low border-none rounded-xl focus:ring-2 focus:ring-primary focus:bg-surface-container-lowest transition-all placeholder:text-outline outline-none"
            placeholder="Tìm kiếm tên thuốc, thành phần hoặc triệu chứng..."
            type="text"
            value={searchInput}
            onChange={(e) => onSearchChange(e.target.value)}
          />
        </div>
        <div className="flex gap-3 w-full md:w-auto">
          <select
            className="appearance-none bg-surface-container-low border-none rounded-xl py-4 pl-6 pr-12 font-medium focus:ring-2 focus:ring-primary outline-none"
            value={sortOption}
            onChange={(e) => onSortChange(e.target.value)}
          >
            <option value="popular">Sắp xếp: Phổ biến nhất</option>
            <option value="price_asc">Giá: Thấp đến Cao</option>
            <option value="price_desc">Giá: Cao đến Thấp</option>
          </select>
        </div>
      </div>
    </>
  );
}
