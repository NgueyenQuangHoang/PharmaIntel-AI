// =============================================================================
// EF Core Configuration: RefreshToken
// Chuc nang: Cau hinh bang refresh_tokens - hash unique, index theo user, audit fields.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Infrastructure.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TokenHash).HasMaxLength(128).IsRequired();
        builder.Property(e => e.ExpiresAt).HasColumnType("datetime2(0)").IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(e => e.CreatedByIp).HasMaxLength(64);
        builder.Property(e => e.UserAgent).HasMaxLength(512);
        builder.Property(e => e.RevokedAt).HasColumnType("datetime2(0)");
        builder.Property(e => e.RevokedByIp).HasMaxLength(64);
        builder.Property(e => e.RevokedReason).HasMaxLength(32);

        builder.Ignore(e => e.IsActive);

        builder.HasIndex(e => e.TokenHash)
            .IsUnique()
            .HasDatabaseName("UQ_refresh_tokens_token_hash");

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("IX_refresh_tokens_user_id");

        builder.HasOne(e => e.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.ReplacedByToken)
            .WithMany()
            .HasForeignKey(e => e.ReplacedByTokenId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.ToTable(t => t.HasCheckConstraint("CK_refresh_tokens_revoked_reason",
            "[revoked_reason] IS NULL OR [revoked_reason] IN ('rotated','logout','theft_detected')"));
    }
}
