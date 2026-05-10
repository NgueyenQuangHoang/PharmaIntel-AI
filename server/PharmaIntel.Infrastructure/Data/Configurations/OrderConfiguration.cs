// =============================================================================
// EF Core Configuration: Order
// Chuc nang: Cau hinh bang orders - snapshot, check status/payment_status, index.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.OrderCode).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Subtotal).HasColumnType("decimal(12,2)").IsRequired();
        builder.Property(e => e.ShippingFee).HasColumnType("decimal(12,2)").IsRequired().HasDefaultValue(0m);
        builder.Property(e => e.Total).HasColumnType("decimal(12,2)").IsRequired();
        builder.Property(e => e.ShippingRecipientName).HasMaxLength(255);
        builder.Property(e => e.ShippingPhone).HasMaxLength(20);
        builder.Property(e => e.ShippingFullAddress).HasMaxLength(1000);
        builder.Property(e => e.PaymentTypeSnapshot).HasMaxLength(30);
        builder.Property(e => e.PaymentStatus).HasMaxLength(20).IsRequired().HasDefaultValue("unpaid");
        builder.Property(e => e.Status).HasMaxLength(20).IsRequired().HasDefaultValue("pending");
        builder.Property(e => e.CreatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(e => e.UpdatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(e => e.OrderCode).IsUnique().HasDatabaseName("UQ_orders_order_code");
        builder.HasIndex(e => new { e.UserId, e.CreatedAt }).HasDatabaseName("IX_orders_user_created_at");

        // Restrict: don hang phai duoc giu de doi soat doanh thu/hoan tien
        // ngay ca khi user bi xoa. Dung soft delete (set is_active=false) o tang code.
        builder.HasOne(e => e.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Address)
            .WithMany(a => a.Orders)
            .HasForeignKey(e => e.AddressId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(e => e.PaymentMethod)
            .WithMany(pm => pm.Orders)
            .HasForeignKey(e => e.PaymentMethodId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(e => e.Prescription)
            .WithMany(p => p.Orders)
            .HasForeignKey(e => e.PrescriptionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_orders_status",
                "[status] IN ('pending','confirmed','processing','shipping','delivered','cancelled','refunded')");
            t.HasCheckConstraint("CK_orders_payment_status",
                "[payment_status] IN ('unpaid','pending','paid','failed','refunded','cod_pending')");
            t.HasCheckConstraint("CK_orders_total", "[total] >= 0");
        });
    }
}
