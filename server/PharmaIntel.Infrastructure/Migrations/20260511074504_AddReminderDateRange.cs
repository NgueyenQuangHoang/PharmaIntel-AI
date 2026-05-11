using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmaIntel.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReminderDateRange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "end_date",
                table: "medication_reminders",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "start_date",
                table: "medication_reminders",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddCheckConstraint(
                name: "CK_medication_reminders_date_range",
                table: "medication_reminders",
                sql: "[end_date] IS NULL OR [end_date] >= [start_date]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_medication_reminders_date_range",
                table: "medication_reminders");

            migrationBuilder.DropColumn(
                name: "end_date",
                table: "medication_reminders");

            migrationBuilder.DropColumn(
                name: "start_date",
                table: "medication_reminders");
        }
    }
}
