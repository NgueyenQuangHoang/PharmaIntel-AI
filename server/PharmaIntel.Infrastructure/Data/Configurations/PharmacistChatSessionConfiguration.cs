// =============================================================================
// EF Core Configuration: PharmacistChatSession
// Chuc nang: Cau hinh bang pharmacist_chat_sessions - check status.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Infrastructure.Data.Configurations;

public class PharmacistChatSessionConfiguration : IEntityTypeConfiguration<PharmacistChatSession>
{
    public void Configure(EntityTypeBuilder<PharmacistChatSession> builder)
    {
        builder.ToTable("pharmacist_chat_sessions");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status).HasMaxLength(20).IsRequired().HasDefaultValue("open");
        builder.Property(e => e.StartedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(e => e.ClosedAt).HasColumnType("datetime2(0)");

        // Restrict: chat voi duoc si lien quan thuoc ke don - co the la bang chung tranh chap y te.
        builder.HasOne(e => e.User)
            .WithMany(u => u.ChatSessions)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Pharmacist)
            .WithMany(p => p.ChatSessions)
            .HasForeignKey(e => e.PharmacistId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.ToTable(t => t.HasCheckConstraint("CK_pharmacist_chat_sessions_status",
            "[status] IN ('open','waiting','closed','cancelled')"));
    }
}
