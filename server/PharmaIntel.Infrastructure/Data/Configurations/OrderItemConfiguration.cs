// =============================================================================
// EF Core Configuration: OrderItem
// Chuc nang: Cau hinh bang order_items - snapshot gia, check so luong/gia.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Infrastructure.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.MedicationNameSnapshot).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Quantity).IsRequired();
        builder.Property(e => e.UnitPrice).HasColumnType("decimal(12,2)").IsRequired();
        builder.Property(e => e.DiscountPercent).HasColumnType("decimal(5,2)").IsRequired().HasDefaultValue(0m);
        builder.Property(e => e.TotalPrice).HasColumnType("decimal(12,2)").IsRequired();

        builder.HasOne(e => e.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Medication)
            .WithMany(m => m.OrderItems)
            .HasForeignKey(e => e.MedicationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_order_items_quantity", "[quantity] > 0");
            t.HasCheckConstraint("CK_order_items_price", "[unit_price] >= 0");
            t.HasCheckConstraint("CK_order_items_total", "[total_price] >= 0");
        });
    }
}
