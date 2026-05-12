using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmaIntel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPrescriptionItemDispensedFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_prescription_items_prescription_id",
                table: "prescription_items");

            migrationBuilder.AddColumn<bool>(
                name: "is_dispensed",
                table: "prescription_items",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_prescription_items_prescription_dispensed",
                table: "prescription_items",
                columns: new[] { "prescription_id", "is_dispensed" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_prescription_items_prescription_dispensed",
                table: "prescription_items");

            migrationBuilder.DropColumn(
                name: "is_dispensed",
                table: "prescription_items");

            migrationBuilder.CreateIndex(
                name: "IX_prescription_items_prescription_id",
                table: "prescription_items",
                column: "prescription_id");
        }
    }
}
