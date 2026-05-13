using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmaIntel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionHardening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "error_type",
                table: "rag_traces",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "generation_latency_ms",
                table: "rag_traces",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "retrieval_latency_ms",
                table: "rag_traces",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "total_latency_ms",
                table: "rag_traces",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "embedding_cache",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    text_hash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    vector_json = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_embedding_cache", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rag_jobs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    job_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "queued"),
                    document_id = table.Column<long>(type: "bigint", nullable: true),
                    payload_json = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}"),
                    error_message = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    started_at = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    completed_at = table.Column<DateTime>(type: "datetime2(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rag_jobs", x => x.id);
                    table.CheckConstraint("CK_rag_jobs_status", "[status] IN ('queued','running','completed','failed')");
                    table.CheckConstraint("CK_rag_jobs_type", "[job_type] IN ('ingest','reindex','delete_vector')");
                });

            migrationBuilder.CreateIndex(
                name: "UQ_embedding_cache_hash_model",
                table: "embedding_cache",
                columns: new[] { "text_hash", "model" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rag_jobs_status_created_at",
                table: "rag_jobs",
                columns: new[] { "status", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "embedding_cache");

            migrationBuilder.DropTable(
                name: "rag_jobs");

            migrationBuilder.DropColumn(
                name: "error_type",
                table: "rag_traces");

            migrationBuilder.DropColumn(
                name: "generation_latency_ms",
                table: "rag_traces");

            migrationBuilder.DropColumn(
                name: "retrieval_latency_ms",
                table: "rag_traces");

            migrationBuilder.DropColumn(
                name: "total_latency_ms",
                table: "rag_traces");
        }
    }
}
