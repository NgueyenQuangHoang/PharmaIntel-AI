// =============================================================================
// EF Core Configuration: Doctor
// Chuc nang: Cau hinh bang doctors.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Infrastructure.Data.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.ToTable("doctors");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.FullName).HasMaxLength(255).IsRequired();
        builder.Property(e => e.LicenseNumber).HasMaxLength(100);
        builder.Property(e => e.Specialization).HasMaxLength(255);
        builder.Property(e => e.Hospital).HasMaxLength(255);
        builder.Property(e => e.Phone).HasMaxLength(20);
        builder.Property(e => e.Email).HasMaxLength(255);
        builder.Property(e => e.AvatarUrl).HasMaxLength(500);
        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(e => e.CreatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(e => e.UpdatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");
    }
}
