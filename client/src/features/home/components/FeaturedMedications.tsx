export function FeaturedMedications() {
  return (
    <section className="bg-surface py-24 px-8">
      <div className="max-w-7xl mx-auto">
        <div className="flex flex-col md:flex-row justify-between items-end mb-16 gap-6">
          <div className="max-w-2xl">
            <h2 className="text-4xl font-extrabold font-headline text-on-surface mb-4 tracking-tight">Tủ thuốc Nổi bật</h2>
            <p className="text-on-surface-variant leading-relaxed">Dược phẩm được kiểm định chất lượng nghiêm ngặt bởi Hội đồng Y khoa Quốc tế.</p>
          </div>
          <button className="text-primary font-bold flex items-center gap-2 group">
            Xem tất cả danh mục <span className="material-symbols-outlined group-hover:translate-x-1 transition-transform">arrow_forward</span>
          </button>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-12 gap-6">
          {/* Large Feature Card */}
          <div className="md:col-span-8 bg-surface-container-lowest rounded-3xl p-8 border border-outline-variant/15 flex flex-col md:flex-row gap-8 items-center overflow-hidden">
            <div className="flex-1 space-y-6">
              <span className="px-3 py-1 bg-tertiary-fixed text-on-tertiary-fixed text-xs font-bold rounded-full uppercase tracking-widest">Bán chạy nhất</span>
              <h3 className="text-3xl font-bold font-headline text-on-surface">Vitality Plus Multivitamin</h3>
              <p className="text-on-surface-variant">Phức hợp vitamin tổng hợp với công nghệ giải phóng chậm giúp duy trì năng lượng suốt 12 giờ.</p>
              <div className="flex items-center gap-4">
                <span className="text-2xl font-bold text-primary">550.000đ</span>
                <button className="bg-primary text-on-primary px-6 py-2 rounded-full font-semibold hover:opacity-90 transition-opacity">Thêm vào giỏ</button>
              </div>
            </div>
            <div className="flex-1">
              <img alt="Medicine Bottle" className="w-full h-64 object-contain" data-alt="..." src="https://lh3.googleusercontent.com/aida-public/AB6AXuD0sEiq2tcqiVyVMMFPcvVFkJtl7swdj7Yy4JxdAC4heRbkv3vZAg7ACX0Jj7st_Ygx7AAAIhcqZczL8laZaLFoD8FFp68n-xrc_U1aWrrHVBy7m4zGGK0oT3iaKGFvxLFALirIHBsWm0lwEqpqYlnka1BnRNdJrU9mV3sXbyAKysbwGJCZB9_93rwqA3XV1FDe-Z0bZyjv8s56ctDHbkVzY1H4Z8DfEgjaClDGHDRSL6b9n-EdyL6-r1-nrFESjx9LarBpy6eZXn4" />
            </div>
          </div>
          
          {/* Side Card 1 */}
          <div className="md:col-span-4 bg-secondary-fixed text-on-secondary-fixed rounded-3xl p-8 flex flex-col justify-between">
            <div className="space-y-4">
              <span className="material-symbols-outlined text-4xl" style={{ fontVariationSettings: "'FILL' 1" }}>spa</span>
              <h3 className="text-2xl font-bold font-headline">Dược liệu Tự nhiên</h3>
              <p className="text-on-secondary-fixed-variant opacity-80">Chiết xuất 100% thảo dược organic.</p>
            </div>
            <button className="mt-8 w-full py-3 bg-on-secondary-fixed text-white rounded-xl font-bold">Khám phá ngay</button>
          </div>
          
          {/* Small Card 2 */}
          <div className="md:col-span-4 bg-surface-container-low rounded-3xl p-8 border border-outline-variant/10">
            <img alt="Eye Drops" className="w-full h-40 object-contain mb-6" data-alt="..." src="https://lh3.googleusercontent.com/aida-public/AB6AXuBIDcq-S3jN_oWjMLrp8U_e7u8E0z0eVKXhZ3yUfQlF3C_LhVhSyS6socg0UxjPkYR2I15s7z7TNSmot7brwNZ4cNk52S3F4A5uTIUZKyjYMlG7z8VkRO5oiSztxusFrP0omT8TUKOOqmJ3Xv52aCmFgERx_ncsIQgetrEcNUtdO-b3nMB8LxBqVUVU3soSaLENj3dwKrQTmnoWvflIcAscDkp4zn3pboEd1QxFVwDTwLvFEhnfpSb_ygOMF-zIaX80tq2UBajtRYo" />
            <h4 className="text-xl font-bold font-headline text-on-surface mb-2">OptiVision Drops</h4>
            <div className="flex justify-between items-center">
              <span className="font-bold text-primary">125.000đ</span>
              <span className="material-symbols-outlined text-outline">add_circle</span>
            </div>
          </div>
          
          {/* Small Card 3 */}
          <div className="md:col-span-4 bg-surface-container-lowest rounded-3xl p-8 border border-outline-variant/15 relative overflow-hidden">
            <div className="absolute top-0 right-0 p-4">
              <span className="px-2 py-1 bg-error-container text-on-error-container text-[10px] font-bold rounded-lg uppercase">-15%</span>
            </div>
            <img alt="First Aid Kit" className="w-full h-40 object-contain mb-6" data-alt="..." src="https://lh3.googleusercontent.com/aida-public/AB6AXuBb_43XE7i4sLJO7wZsTCm63QCCCojYMcjr-3_IQ-sY8xxzEWm0Em4h--m9vKUg1XFz4dmKI1DrE-_X8Y0w-zvJJKyy-QJNZNShcGFN2j7ucjnOrgikO0kn4QaPTmGoKie4j23KWtge3S6Gks7A6ISACVyME2q8_iAxAHow_I0qlgKUUHyqC3gk9qfdrt6lVLENGYDnSG6g9Tagt2dEO7SvZhMe3FAyOQdzE38mmyBvuMQXbhtNnDix-LM-RU2qnNCpHre_HWMklH8" />
            <h4 className="text-xl font-bold font-headline text-on-surface mb-2">Home Safety Kit</h4>
            <div className="flex justify-between items-center">
              <span className="font-bold text-primary">890.000đ</span>
              <span className="material-symbols-outlined text-outline">add_circle</span>
            </div>
          </div>
          
          {/* Small Card 4 */}
          <div className="md:col-span-4 bg-surface-container-low rounded-3xl p-8 border border-outline-variant/10 group cursor-pointer hover:bg-primary transition-colors">
            <div className="h-full flex flex-col justify-center items-center text-center space-y-4">
              <div className="w-16 h-16 rounded-full bg-white flex items-center justify-center text-primary group-hover:scale-110 transition-transform">
                <span className="material-symbols-outlined text-3xl">category</span>
              </div>
              <h4 className="text-xl font-bold font-headline group-hover:text-white">Tất cả sản phẩm</h4>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
