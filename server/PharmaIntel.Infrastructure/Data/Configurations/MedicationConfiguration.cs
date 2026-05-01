// =============================================================================
// EF Core Configuration: Medication
// Chuc nang: Cau hinh bang medications - unique SKU, check gia/so luong/giam gia.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Infrastructure.Data.Configurations;

public class MedicationConfiguration : IEntityTypeConfiguration<Medication>
{
    public void Configure(EntityTypeBuilder<Medication> builder)
    {
        builder.ToTable("medications");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Sku).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(255).IsRequired();
        builder.Property(e => e.GenericName).HasMaxLength(255);
        builder.Property(e => e.Manufacturer).HasMaxLength(255);
        builder.Property(e => e.RegistrationNumber).HasMaxLength(100);
        builder.Property(e => e.Description).HasColumnType("nvarchar(max)");
        builder.Property(e => e.Dosage).HasMaxLength(100);
        builder.Property(e => e.Packaging).HasMaxLength(100);
        builder.Property(e => e.Price).HasColumnType("decimal(12,2)").IsRequired();
        builder.Property(e => e.DiscountPercent).HasColumnType("decimal(5,2)").IsRequired().HasDefaultValue(0m);
        builder.Property(e => e.UsageInstructions).HasColumnType("nvarchar(max)");
        builder.Property(e => e.Benefits).HasColumnType("nvarchar(max)");
        builder.Property(e => e.ActiveIngredients).HasColumnType("nvarchar(max)");
        builder.Property(e => e.Contraindications).HasColumnType("nvarchar(max)");
        builder.Property(e => e.SideEffects).HasColumnType("nvarchar(max)");
        builder.Property(e => e.StorageInstructions).HasColumnType("nvarchar(max)");
        builder.Property(e => e.ImageUrl).HasMaxLength(500);
        builder.Property(e => e.IsFeatured).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.IsBestSeller).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.IsPrescriptionRequired).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.StockQuantity).IsRequired().HasDefaultValue(0);
        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(e => e.CreatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(e => e.UpdatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(e => e.Sku).IsUnique().HasDatabaseName("UQ_medications_sku");

        builder.HasOne(e => e.Category)
            .WithMany(c => c.Medications)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_medications_price", "[price] >= 0");
            t.HasCheckConstraint("CK_medications_discount", "[discount_percent] >= 0 AND [discount_percent] <= 100");
            t.HasCheckConstraint("CK_medications_stock", "[stock_quantity] >= 0");
        });
    }
}
