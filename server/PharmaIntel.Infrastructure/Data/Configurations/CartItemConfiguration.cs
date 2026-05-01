// =============================================================================
// EF Core Configuration: CartItem
// Chuc nang: Cau hinh bang cart_items - unique user+medication, check quantity.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Infrastructure.Data.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("cart_items");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Quantity).IsRequired();
        builder.Property(e => e.AddedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(e => e.User)
            .WithMany(u => u.CartItems)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Medication)
            .WithMany(m => m.CartItems)
            .HasForeignKey(e => e.MedicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.UserId, e.MedicationId })
            .IsUnique()
            .HasDatabaseName("UQ_cart_items_user_medication");

        builder.ToTable(t => t.HasCheckConstraint("CK_cart_items_quantity", "[quantity] > 0"));
    }
}
