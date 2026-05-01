// =============================================================================
// EF Core Configuration: UserSetting
// Chuc nang: Cau hinh bang user_settings - 1:1 voi users, check dark_mode.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Infrastructure.Data.Configurations;

public class UserSettingConfiguration : IEntityTypeConfiguration<UserSetting>
{
    public void Configure(EntityTypeBuilder<UserSetting> builder)
    {
        builder.ToTable("user_settings");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.DarkMode).HasMaxLength(20).IsRequired().HasDefaultValue("system");
        builder.Property(e => e.LanguageCode).HasMaxLength(10).IsRequired().HasDefaultValue("vi");
        builder.Property(e => e.NotificationEnabled).IsRequired().HasDefaultValue(true);
        builder.Property(e => e.ReminderSoundEnabled).IsRequired().HasDefaultValue(true);
        builder.Property(e => e.CreatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(e => e.UpdatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(e => e.UserId).IsUnique().HasDatabaseName("UQ_user_settings_user_id");

        builder.HasOne(e => e.User)
            .WithOne(u => u.Setting)
            .HasForeignKey<UserSetting>(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(t => t.HasCheckConstraint("CK_user_settings_dark_mode",
            "[dark_mode] IN ('light','dark','system')"));
    }
}
