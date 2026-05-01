// =============================================================================
// EF Core Configuration: Address
// Chuc nang: Cau hinh bang addresses - filtered unique index cho dia chi mac dinh.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Infrastructure.Data.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("addresses");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.RecipientName).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Phone).HasMaxLength(20).IsRequired();
        builder.Property(e => e.Province).HasMaxLength(100).IsRequired();
        builder.Property(e => e.District).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Ward).HasMaxLength(100).IsRequired();
        builder.Property(e => e.StreetAddress).HasMaxLength(500).IsRequired();
        builder.Property(e => e.IsDefault).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(e => e.CreatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(e => e.UpdatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(e => e.User)
            .WithMany(u => u.Addresses)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.UserId, e.IsDefault })
            .IsUnique()
            .HasFilter("[is_default] = 1")
            .HasDatabaseName("UX_addresses_user_default");
    }
}
