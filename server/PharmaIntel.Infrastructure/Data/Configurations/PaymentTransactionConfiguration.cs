// =============================================================================
// EF Core Configuration: PaymentTransaction
// Chuc nang: Cau hinh bang payment_transactions - check status, index.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Infrastructure.Data.Configurations;

public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.ToTable("payment_transactions");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Provider).HasMaxLength(50);
        builder.Property(e => e.ProviderTransactionId).HasMaxLength(255);
        builder.Property(e => e.Amount).HasColumnType("decimal(12,2)").IsRequired();
        builder.Property(e => e.Status).HasMaxLength(20).IsRequired().HasDefaultValue("initiated");
        builder.Property(e => e.CreatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(e => e.UpdatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(e => e.Order)
            .WithMany(o => o.Transactions)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.PaymentMethod)
            .WithMany(pm => pm.Transactions)
            .HasForeignKey(e => e.PaymentMethodId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(e => e.OrderId).HasDatabaseName("IX_payment_transactions_order_id");

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_payment_transactions_status",
                "[status] IN ('initiated','pending','success','failed','cancelled','refunded')");
            t.HasCheckConstraint("CK_payment_transactions_amount", "[amount] >= 0");
        });
    }
}
