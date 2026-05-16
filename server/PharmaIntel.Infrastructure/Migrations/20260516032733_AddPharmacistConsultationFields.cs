using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmaIntel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPharmacistConsultationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "about",
                table: "pharmacists",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "experience_years",
                table: "pharmacists",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "rating",
                table: "pharmacists",
                type: "decimal(3,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "reviews_count",
                table: "pharmacists",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_pharmacists_experience_years_nonneg",
                table: "pharmacists",
                sql: "[experience_years] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_pharmacists_rating_range",
                table: "pharmacists",
                sql: "[rating] >= 0 AND [rating] <= 5");

            migrationBuilder.AddCheckConstraint(
                name: "CK_pharmacists_reviews_count_nonneg",
                table: "pharmacists",
                sql: "[reviews_count] >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_pharmacists_experience_years_nonneg",
                table: "pharmacists");

            migrationBuilder.DropCheckConstraint(
                name: "CK_pharmacists_rating_range",
                table: "pharmacists");

            migrationBuilder.DropCheckConstraint(
                name: "CK_pharmacists_reviews_count_nonneg",
                table: "pharmacists");

            migrationBuilder.DropColumn(
                name: "about",
                table: "pharmacists");

            migrationBuilder.DropColumn(
                name: "experience_years",
                table: "pharmacists");

            migrationBuilder.DropColumn(
                name: "rating",
                table: "pharmacists");

            migrationBuilder.DropColumn(
                name: "reviews_count",
                table: "pharmacists");
        }
    }
}
