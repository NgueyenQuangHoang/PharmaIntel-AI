// =============================================================================
// EF Core Configuration: PaymentMethod
// Chuc nang: Cau hinh bang payment_methods - check payment_type, filtered unique default.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Infrastructure.Data.Configurations;

public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.ToTable("payment_methods");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.PaymentType).HasMaxLength(30).IsRequired();
        builder.Property(e => e.DisplayName).HasMaxLength(255).IsRequired();
        builder.Property(e => e.MaskedAccount).HasMaxLength(255);
        builder.Property(e => e.ProviderCustomerId).HasMaxLength(255);
        builder.Property(e => e.IsDefault).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(e => e.CreatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(e => e.UpdatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(e => e.User)
            .WithMany(u => u.PaymentMethods)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.UserId, e.IsDefault })
            .IsUnique()
            .HasFilter("[is_default] = 1")
            .HasDatabaseName("UX_payment_methods_user_default");

        builder.ToTable(t => t.HasCheckConstraint("CK_payment_methods_type",
            "[payment_type] IN ('cod','bank_transfer','momo','zalopay','vnpay','credit_card')"));
    }
}
