using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmaIntel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReminderItemTimeUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_medication_reminders_prescription_item_id",
                table: "medication_reminders");

            migrationBuilder.CreateIndex(
                name: "UX_medication_reminders_item_time",
                table: "medication_reminders",
                columns: new[] { "prescription_item_id", "reminder_time" },
                unique: true,
                filter: "[prescription_item_id] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_medication_reminders_item_time",
                table: "medication_reminders");

            migrationBuilder.CreateIndex(
                name: "IX_medication_reminders_prescription_item_id",
                table: "medication_reminders",
                column: "prescription_item_id");
        }
    }
}
