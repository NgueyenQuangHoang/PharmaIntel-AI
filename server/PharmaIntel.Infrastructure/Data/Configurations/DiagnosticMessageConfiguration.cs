// =============================================================================
// EF Core Configuration: DiagnosticMessage
// Chuc nang: Cau hinh bang diagnostic_messages - check sender_type.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Infrastructure.Data.Configurations;

public class DiagnosticMessageConfiguration : IEntityTypeConfiguration<DiagnosticMessage>
{
    public void Configure(EntityTypeBuilder<DiagnosticMessage> builder)
    {
        builder.ToTable("diagnostic_messages");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.SenderType).HasMaxLength(20).IsRequired();
        builder.Property(e => e.Content).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(e => e.SentAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(e => e.Session)
            .WithMany(s => s.Messages)
            .HasForeignKey(e => e.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(t => t.HasCheckConstraint("CK_diagnostic_messages_sender",
            "[sender_type] IN ('user','ai','system')"));
    }
}
