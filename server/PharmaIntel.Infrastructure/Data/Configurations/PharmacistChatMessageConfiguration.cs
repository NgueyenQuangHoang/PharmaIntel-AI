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

        builder.ToTable(t => t.HasCheckConstraint("CK_pharmacist_chat_messages_sender",
            "[sender_type] IN ('user','pharmacist','system')"));
    }
}
