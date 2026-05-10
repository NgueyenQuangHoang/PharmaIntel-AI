// =============================================================================
// EF Core Configuration: PharmacistChatMessage
// Chuc nang: Cau hinh bang pharmacist_chat_messages - check sender_type.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Infrastructure.Data.Configurations;

public class PharmacistChatMessageConfiguration : IEntityTypeConfiguration<PharmacistChatMessage>
{
    public void Configure(EntityTypeBuilder<PharmacistChatMessage> builder)
    {
        builder.ToTable("pharmacist_chat_messages");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.SenderType).HasMaxLength(20).IsRequired();
        builder.Property(e => e.Content).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(e => e.SentAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(e => e.Session)
            .WithMany(s => s.Messages)
            .HasForeignKey(e => e.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        // FK to User (nullable). Restrict de giu lich su chat khi user bi xoa mem.
        builder.HasOne(e => e.SenderUser)
            .WithMany()
            .HasForeignKey(e => e.SenderUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // FK to Pharmacist (nullable). Restrict de giu lich su.
        builder.HasOne(e => e.SenderPharmacist)
            .WithMany()
            .HasForeignKey(e => e.SenderPharmacistId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_pharmacist_chat_messages_sender",
                "[sender_type] IN ('user','pharmacist','system')");

            // Dam bao sender_type khop voi sender_*_id:
            //   user      -> sender_user_id NOT NULL,        sender_pharmacist_id NULL
            //   pharmacist-> sender_user_id NULL,            sender_pharmacist_id NOT NULL
            //   system    -> ca hai NULL
            t.HasCheckConstraint("CK_pharmacist_chat_messages_sender_consistency", @"
                ([sender_type] = 'user'       AND [sender_user_id] IS NOT NULL AND [sender_pharmacist_id] IS NULL)
             OR ([sender_type] = 'pharmacist' AND [sender_user_id] IS NULL     AND [sender_pharmacist_id] IS NOT NULL)
             OR ([sender_type] = 'system'     AND [sender_user_id] IS NULL     AND [sender_pharmacist_id] IS NULL)");
        });
    }
}
