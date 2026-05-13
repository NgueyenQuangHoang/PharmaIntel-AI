// =============================================================================
// EF Config: AiResponseFeedback
// =============================================================================
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaIntel.Core.Entities;

namespace PharmaIntel.Infrastructure.Data.Configurations;

public class AiResponseFeedbackConfiguration : IEntityTypeConfiguration<AiResponseFeedback>
{
    public void Configure(EntityTypeBuilder<AiResponseFeedback> builder)
    {
        builder.ToTable("ai_response_feedbacks");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Rating)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.ReasonType)
            .HasMaxLength(50);

        builder.Property(x => x.Comment)
            .HasMaxLength(2000);

        builder.Property(x => x.AdminNote)
            .HasMaxLength(2000);

        builder.Property(x => x.IsReviewed)
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(x => x.ReviewedAt)
            .HasColumnType("datetime2(0)");

        builder.HasIndex(x => new { x.Rating, x.IsReviewed })
            .HasDatabaseName("IX_ai_feedback_rating_reviewed");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("IX_ai_feedback_created_at");

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_ai_feedback_rating",
                "[rating] IN ('thumbs_up','thumbs_down')");

            t.HasCheckConstraint(
                "CK_ai_feedback_reason_type",
                "[reason_type] IS NULL OR [reason_type] IN ('wrong_medication','unsafe_advice','not_helpful','hallucination','other')");
        });
    }
}
