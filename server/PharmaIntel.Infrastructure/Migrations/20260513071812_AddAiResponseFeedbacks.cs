using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmaIntel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAiResponseFeedbacks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ai_response_feedbacks",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    diagnostic_session_id = table.Column<long>(type: "bigint", nullable: true),
                    diagnostic_message_id = table.Column<long>(type: "bigint", nullable: true),
                    rag_trace_id = table.Column<long>(type: "bigint", nullable: true),
                    rating = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    reason_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    is_reviewed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    admin_note = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    reviewed_at = table.Column<DateTime>(type: "datetime2(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_response_feedbacks", x => x.id);
                    table.CheckConstraint("CK_ai_feedback_rating", "[rating] IN ('thumbs_up','thumbs_down')");
                    table.CheckConstraint("CK_ai_feedback_reason_type", "[reason_type] IS NULL OR [reason_type] IN ('wrong_medication','unsafe_advice','not_helpful','hallucination','other')");
                    table.ForeignKey(
                        name: "FK_ai_response_feedbacks_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ai_feedback_created_at",
                table: "ai_response_feedbacks",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_ai_feedback_rating_reviewed",
                table: "ai_response_feedbacks",
                columns: new[] { "rating", "is_reviewed" });

            migrationBuilder.CreateIndex(
                name: "IX_ai_response_feedbacks_user_id",
                table: "ai_response_feedbacks",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_response_feedbacks");
        }
    }
}
