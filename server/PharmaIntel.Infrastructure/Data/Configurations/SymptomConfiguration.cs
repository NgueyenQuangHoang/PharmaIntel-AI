// =============================================================================
// EF Core Configuration: Symptom
// Chuc nang: Cau hinh bang symptoms - unique name.
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Infrastructure.Data.Configurations;

public class SymptomConfiguration : IEntityTypeConfiguration<Symptom>
{
    public void Configure(EntityTypeBuilder<Symptom> builder)
    {
        builder.ToTable("symptoms");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name).HasMaxLength(255).IsRequired();
        builder.Property(e => e.GroupName).HasMaxLength(100);
        builder.Property(e => e.DisplayOrder).IsRequired().HasDefaultValue(0);

        builder.HasIndex(e => e.Name).IsUnique().HasDatabaseName("UQ_symptoms_name");
    }
}
