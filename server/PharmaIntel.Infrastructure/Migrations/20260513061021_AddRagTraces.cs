using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmaIntel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRagTraces : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "rag_traces",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    diagnostic_session_id = table.Column<long>(type: "bigint", nullable: true),
                    user_message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    medication_context_json = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "[]"),
                    knowledge_context_json = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "[]"),
                    ai_response = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    has_medication_context = table.Column<bool>(type: "bit", nullable: false),
                    has_knowledge_context = table.Column<bool>(type: "bit", nullable: false),
                    has_red_flag_warning = table.Column<bool>(type: "bit", nullable: false),
                    has_suggested_medication = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rag_traces", x => x.id);
                    table.ForeignKey(
                        name: "FK_rag_traces_diagnostic_sessions_diagnostic_session_id",
                        column: x => x.diagnostic_session_id,
                        principalTable: "diagnostic_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_rag_traces_created_at",
                table: "rag_traces",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_rag_traces_session_id",
                table: "rag_traces",
                column: "diagnostic_session_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rag_traces");
        }
    }
}
