// =============================================================================
// EF Core Configuration: Consultation
// Chuc nang: Cau hinh bang consultations - check status, index theo pharmacist
// va user de truy van trung lich + dashboard.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Infrastructure.Data.Configurations;

public class ConsultationConfiguration : IEntityTypeConfiguration<Consultation>
{
    public void Configure(EntityTypeBuilder<Consultation> builder)
    {
        builder.ToTable("consultations");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ScheduledAt).HasColumnType("datetime2(0)").IsRequired();
        builder.Property(e => e.Note).HasMaxLength(1000);
        builder.Property(e => e.Status).HasMaxLength(20).IsRequired().HasDefaultValue("pending");
        builder.Property(e => e.ResponseNote).HasMaxLength(1000);
        builder.Property(e => e.CreatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(e => e.UpdatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Pharmacist)
            .WithMany()
            .HasForeignKey(e => e.PharmacistId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.PharmacistId, e.ScheduledAt })
            .HasDatabaseName("IX_consultations_pharmacist_scheduled");
        builder.HasIndex(e => new { e.UserId, e.ScheduledAt })
            .HasDatabaseName("IX_consultations_user_scheduled");

        builder.ToTable(t => t.HasCheckConstraint("CK_consultations_status",
            "[status] IN ('pending','accepted','rejected','completed','cancelled')"));
    }
}
